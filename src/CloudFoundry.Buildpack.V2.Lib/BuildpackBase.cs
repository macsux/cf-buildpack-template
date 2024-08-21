using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using NMica.Utils.IO;
using Semver;
using Serilog;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public abstract class BuildpackBase
{
    public ILogger Logger { get; internal set; } = Serilog.Log.Logger;
    internal SpecVersionMode SpecVersionMode { get; set; } = SpecVersionMode.V2; 
    /// <summary>
    /// Dictionary of environmental variables to be set at runtime before the app starts
    /// </summary>
    // public Dictionary<string,string> EnvironmentalVariables { get; } = new();
    protected bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    protected abstract string ImplementingClassName { get; }
    protected virtual string? BuildpackVersion { get; }
        
    /// <summary>
    /// Determines if the buildpack is compatible and should be applied to the application being staged.
    /// </summary>
    /// <returns>True if buildpack should be applied, otherwise false</returns>
    public abstract DetectResult Detect(DetectContext context);
    public abstract BuildResult Supply(BuildContext context);
    public abstract BuildResult Finalize(BuildContext context);
    public abstract void Release(ReleaseContext context);

    protected virtual void PrintHeader()
    {
        var versionInfo = BuildpackVersion != null ? $"v.{BuildpackVersion}" : "";
        Log.Information("===Applying {BuildpackName} {BuildpackVersion}===",ImplementingClassName, versionInfo);
    }

    /// <summary>
    /// Code that will execute during the run stage before the app is started
    /// </summary>
    public virtual PreStartResult PreStartup(PreStartupContext context)
    {
        return new PreStartResult();
    }

    /// <summary>
    /// Logic to apply when buildpack is ran.
    /// Note that for <see cref="SupplyBuildpack"/> this will correspond to "bin/supply" lifecycle event, while for <see cref="FinalBuildpack"/> it will be invoked on "bin/finalize"
    /// </summary>
    protected abstract BuildResult Apply(BuildContext context);


    internal void DoPreStartup(PreStartupContext context)
    {
        var result = PreStartup(context);
        var profiled = context.AppDirectory / ".profile.d";
        InstallStartupEnvVars(result.EnvironmentalVariables, profiled, context.BuildpackIndex, true);
    }
    protected abstract bool IsPreStartOverridden { get; }
    protected BuildResult DoApply(BuildContext context)
    {
        PrintHeader();
        var result = Apply(context);
        
        // var buildpackDepsDir = Path.Combine(depsPath, index.ToString());
        FileSystemTasks.EnsureExistingDirectory(context.MyDependenciesDirectory.CurrentAbsolutePath);
        var profiled = context.BuildDirectory / ".profile.d";
        FileSystemTasks.EnsureExistingDirectory(profiled.CurrentAbsolutePath);
        InstallStartupEnvVars(result.EnvironmentalVariables, profiled, context.BuildpackIndex, false);
        if (IsPreStartOverridden) 
        {
            // copy buildpack to deps dir so we can invoke it as part of startup

            var buildpackBinFolder = BuildpackRoot.Instance.BinDirectory.CurrentAbsolutePath;
            var buildpackFiles = Directory.EnumerateFiles(buildpackBinFolder)
                .Select(x => (AbsolutePath)x)
                // .Cast<AbsolutePath>()
                .Where(x => !Lifecycle.AllValues.Where(l => l != Lifecycle.PreStartup).Contains(x.NameWithoutExtension)) // copy over all buildpack bin files, excluding all hooks except prestart
                .ToList();
            foreach(var file in buildpackFiles)
            {
                FileSystemTasks.CopyFile(file, context.MyDependenciesDirectory.CurrentAbsolutePath / file.Name);
            }

            var extension = !IsLinux ? ".exe" : string.Empty;
            var prestartCommand = $"prestartup{extension}";
            // write startup shell script to call buildpack prestart lifecycle event in deps dir
            var startupScriptName = $"{context.BuildpackIndex:00}_{ImplementingClassName}_startup";
            if (IsLinux)
            {
                File.WriteAllText(profiled.CurrentAbsolutePath / $"{startupScriptName}.sh", $"#!/bin/bash\n$DEPS_DIR/{context.BuildpackIndex}/{prestartCommand} {context.BuildpackIndex}");
            }
            else
            {
                File.WriteAllText(profiled.CurrentAbsolutePath / $"{startupScriptName}.bat", $@"%DEPS_DIR%\{context.BuildpackIndex}\{prestartCommand} {context.BuildpackIndex}");
            }
            GetEnvScriptFile(profiled, context.BuildpackIndex, true); // causes empty env file to be created so it can (potentially) be populated with vars during prestart hook
        }

        return result;
    }
    private AbsolutePath GetEnvScriptFile(VariablePath profiled, int index, bool isPreStart)
    {
        var prefix = isPreStart ? "z" : string.Empty;
        var suffix = IsLinux ? ".sh" : ".bat";
        var envScriptName = profiled.CurrentAbsolutePath / $"{prefix}{index:00}_{ImplementingClassName}_env{suffix}";
        // ensure it's initialized
        if(!File.Exists(envScriptName))
            File.WriteAllText(envScriptName, string.Empty);
        return envScriptName;
    }
    protected void InstallStartupEnvVars(Dictionary<string, ValueAction> environmentalVariables, VariablePath profiled, int index, bool isPreStart)
    {
        var envScriptName = GetEnvScriptFile(profiled, index, isPreStart);
            
        if (environmentalVariables.Any())
        {
            if (IsLinux)
            {
                var envVars = environmentalVariables.Aggregate(new StringBuilder(), (sb,x) =>
                {
                    if(x.Value is SetValueAction setValueAction)
                        return sb.Append($"export {x.Key}={Escape(setValueAction.Value)}\n");
                    var appendValueAction = (AppendValueAction)x.Value;
                    return sb.Append($"export {appendValueAction.Key}=${appendValueAction.Key}{appendValueAction.Delimiter}{Escape(appendValueAction.Value)}\n");

                });
                File.WriteAllText(envScriptName, $"#!/bin/bash\n{envVars}");
            }
            else
            {
                var envVars = environmentalVariables.Aggregate(new StringBuilder(), (sb,x) =>
                {
                    if(x.Value is SetValueAction setValueAction)
                        return sb.Append($"SET {setValueAction.Key}={setValueAction.Value}\r\n");
                    var appendValueAction = (AppendValueAction)x.Value;
                    return sb.Append($"SET {appendValueAction.Key}=%{appendValueAction.Key}%{appendValueAction.Delimiter}{Escape(appendValueAction.Value)}\r\n");
                });
                File.WriteAllText(envScriptName,envVars.ToString());
            }
        }
            
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\\\"")}\"";
}