using System.CommandLine;
using CloudFoundry.Buildpack.V2.Commands;
using NMica.Utils;
using NMica.Utils.IO;
#pragma warning disable CS0162 // Unreachable code detected

namespace CloudFoundry.Buildpack.V2;

public class BuildpackHost
{
    BuildpackBase _buildpack;

    private BuildpackHost(BuildpackBase buildpack)
    {
        _buildpack = buildpack;
    }
    public static BuildpackHost Create<T>() where T : BuildpackBase, new()
    {
        return new BuildpackHost(new T());
    }
    public int Run()
    {
        var args = EnvironmentHelper.GetCommandLineArgsNative();
        //Console.WriteLine(string.Join(" ", args));
        // var args = Environment.GetCommandLineArgs().ToList().ToArray();
        if (EnvironmentInfo.IsWin && args[0].StartsWith(@"\"))
        {
            args[0] = $"c:{args[0]}";
        }
        if(!Path.IsPathRooted(args[0]))
        {
            args[0] = (AbsolutePath)Directory.GetCurrentDirectory() / args[0];
        }
        // Console.WriteLine($"Entry:{Assembly.GetEntryAssembly()?.Location}");
        // Console.WriteLine("------");
        
        // return 0;
        var entrypointExecutable = (AbsolutePath)args[0];
        var hookName = entrypointExecutable.NameWithoutExtension; // try to get hook name from entrypoint executable name (normal execution conditions)
        args = args.Skip(1).ToArray(); // going forward, remove entrypoint as the first arg
        // Console.Error.WriteLine($"HookName: {hookName}");
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
        
        int returnCode = 0;
        Command CreateSupplyCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.Supply, "Provides dependencies for an app");
            command.AddArgument(appDirectory);
            command.AddArgument(cacheDirectory);
            command.AddArgument(dependenciesDirectory);
            command.AddArgument(buildpackIndex);
            command.SetHandler(context => _buildpack.Supply(context), new BuildContextBinder(appDirectory, cacheDirectory, dependenciesDirectory, buildpackIndex, hookName));
            return command;
        }
        
        Command CreateDetectCommand(bool rooted)
        {
            var command = rooted ? new RootCommand() : new Command(Lifecycle.Detect, "Determines whether or not to apply the buildpack to an app");
            command.AddArgument(appDirectory);
            command.SetHandler(context =>
            {
                returnCode = _buildpack.Detect(context) ? 0 : 1;
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
            command.SetHandler(context => _buildpack.Finalize(context), new BuildContextBinder(appDirectory, cacheDirectory, dependenciesDirectory, buildpackIndex, hookName));
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