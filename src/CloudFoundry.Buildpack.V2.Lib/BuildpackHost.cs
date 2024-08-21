using System.CommandLine;
using System.Diagnostics;
using CloudFoundry.Buildpack.V2.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NMica.Utils;
using NMica.Utils.IO;
using Serilog;
using Serilog.Events;

#pragma warning disable CS0162 // Unreachable code detected

namespace CloudFoundry.Buildpack.V2;

public class BuildpackHost
{
    BuildpackBase _buildpack;

    private BuildpackHost(BuildpackBase buildpack)
    {
        _buildpack = buildpack;
        _buildpack.SpecVersionMode = Environment.GetEnvironmentVariable("CNB_BUILD_PLAN_PATH") != null ? SpecVersionMode.V3 : SpecVersionMode.V2;
        
    }
    public static BuildpackHost Create<T>() where T : BuildpackBase, new()
    {
        return new BuildpackHost(new T());
    }
    public int Run()
    {
        var args = EnvironmentHelper.GetCommandLineArgsNative();
        
        var hookName = EnvironmentHelper.EntrypointExecutable.NameWithoutExtension; // try to get hook name from entrypoint executable name (normal execution conditions)
        if (hookName != "release") // can't write anything to console during release phase as stdout is used to communicate startup command
        {
            var logLevel = Environment.GetEnvironmentVariable("BP_LOG_LEVEL")?.ToUpper() switch
            {
                "INFO" => LogEventLevel.Information,
                "DEBUG" => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };
            Log.Logger = new LoggerConfiguration().WriteTo.Console(logLevel, outputTemplate: "{Message:l}{NewLine}").CreateLogger();
            _buildpack.Logger = Log.Logger;
        }

        args = args.Skip(1).ToArray(); // going forward, remove entrypoint as the first arg
        var appDirectory = new Argument<string>(
            name: "buildPath",
            description: "Directory path to the application");
        var cacheDirectory = new Argument<string>(
            name: "cachePath",
            description: "Location the buildpack can use to store assets during the build process that is persistent between builds");
        var dependenciesDirectory = new Argument<string>(
            name: "depsPath",
            description: "Directory where dependencies provided by all buildpacks are installed");
        var buildpackIndex = new Argument<int>(
            name: "index",
            description: "Number that represents the ordinal position of the current buildpack");
        var profiled = new Argument<string>(
            name: "profiled",
            description: "ProfileD location");
        
        Environment.SetEnvironmentVariable(EnvironmentHelper.BuildpackRootEnvironmentalVariableName, EnvironmentHelper.EntrypointExecutable.Parent.Parent);
        
        int returnCode = 0;

        void SetBuildplan(BuildContext context)
        {
            // there's no mechanism to pass metadata between detect and build phases in v2 - we're just gonna call detect again
            if (_buildpack.SpecVersionMode == SpecVersionMode.V2)
            {
                var detectResult = _buildpack.Detect(new DetectContext { BuildDirectory = context.BuildDirectory });
                if (detectResult.Buildplans.Count > 1)
                {
                    throw new Exception("Multiple buildplans not supported in V2 mode");
                }
                context.Buildplan = detectResult.Buildplans.SingleOrDefault() ?? new();
            }
        }

        Command CreateSupplyCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.Supply, "Provides dependencies for an app");
            command.AddArgument(appDirectory);
            command.AddArgument(cacheDirectory);
            command.AddArgument(dependenciesDirectory);
            command.AddArgument(buildpackIndex);
            command.SetHandler(context =>
            {
                SetBuildplan(context);
                _buildpack.Supply(context);
            }, new BuildContextBinder(appDirectory, cacheDirectory, dependenciesDirectory, buildpackIndex, hookName));
            return command;
        }
        Command CreateDetectCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.Detect, "Determines whether or not to apply the buildpack to an app");
            command.AddArgument(appDirectory);
            command.SetHandler(context =>
            {
                // for v2, only one buildpack can participate in detection. supply buildpacks MUST fail detection so they don't conflict with 
                // final buildpacks when installed in the platform (as they can't be used as standalone mode)
                if (_buildpack.SpecVersionMode == SpecVersionMode.V2 && _buildpack is SupplyBuildpack)
                {
                    returnCode = (int)DetectResultCode.Fail;
                    return;
                }
                var result = _buildpack.Detect(context);
                returnCode = result.ResultCode;
            }, new DetectContextBinder(appDirectory));
            return command;
        }
        Command CreateFinalizeCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.Finalize, "Finalizes creation of the container and prepares the app for launch");
            command.AddArgument(appDirectory);
            command.AddArgument(cacheDirectory);
            command.AddArgument(dependenciesDirectory);
            command.AddArgument(buildpackIndex);
            command.AddArgument(profiled);
            command.SetHandler(context =>
            {
                SetBuildplan(context);
                _buildpack.Finalize(context);
            }, new BuildContextBinder(appDirectory, cacheDirectory, dependenciesDirectory, buildpackIndex, hookName));
            return command;
        }
        
        Command CreateReleaseCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.Release, "Provides feedback metadata to Cloud Foundry that indicates how you can run the app");
            command.AddArgument(appDirectory);
            command.SetHandler(context => _buildpack.Release(context), new ReleaseContextBinder(appDirectory));
            return command;
        }
        
        Command CreatePreStartCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.PreStartup, "Executes buildpack code right before application starts");
            command.AddArgument(buildpackIndex);
            command.SetHandler(context => _buildpack.PreStartup(context), new PreStartContextBinder(buildpackIndex));
            
            return command;
        }
        
        RootCommand rootCommand;
        if (Lifecycle.AllValues.Contains(hookName))
        {
            // args[0] = hookName;
            rootCommand = hookName switch
            {
                Lifecycle.Detect => (RootCommand)CreateDetectCommand(true),
                Lifecycle.Supply => (RootCommand)CreateSupplyCommand(true),
                Lifecycle.Finalize => (RootCommand)CreateFinalizeCommand(true),
                Lifecycle.Release => (RootCommand)CreateReleaseCommand(true),
                Lifecycle.PreStartup => (RootCommand)CreatePreStartCommand(true),
                _ => throw new Exception("Invalid hook name")
            };
        }
        else
        {
            // entrypoint is called something else - probably running in debug mode in IDE. in that case, subcommand should be passed as first argument
            rootCommand = new RootCommand();
            rootCommand.AddCommand(CreateDetectCommand(false));
            rootCommand.AddCommand(CreateSupplyCommand(false));
            rootCommand.AddCommand(CreateFinalizeCommand(false));
            rootCommand.AddCommand(CreateReleaseCommand(false));
            rootCommand.AddCommand(CreatePreStartCommand(false));
        }
        
        
        var rootCode = rootCommand.Invoke(args);
        // Console.WriteLine($"Command: {EnvironmentHelper.GetCommandLineArgsNative()[0]}");
        // Console.WriteLine($"ReturnCode: {returnCode}");

        if (rootCode == 0)
            return returnCode;
        return rootCode;
    }

}