using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using NMica.Utils;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public abstract class BuildpackBase
{
    /// <summary>
    /// Dictionary of environmental variables to be set at runtime before the app starts
    /// </summary>
    protected Dictionary<string,string> EnvironmentalVariables { get; } = new();
    protected bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    protected abstract string ImplementingClassName { get; }
    protected virtual string? BuildpackVersion { get; }
        
    /// <summary>
    /// Determines if the buildpack is compatible and should be applied to the application being staged.
    /// </summary>
    /// <returns>True if buildpack should be applied, otherwise false</returns>
    public abstract bool Detect(DetectContext context);
    public abstract void Supply(BuildContext context);
    public abstract void Finalize(BuildContext context);
    public abstract void Release(ReleaseContext context);

    protected virtual void PrintHeader()
    {
        var versionInfo = BuildpackVersion != null ? $"v.{BuildpackVersion}" : "";
        Console.WriteLine($"===Applying {nameof(ImplementingClassName)} {versionInfo}===");        
    }
    /// <summary>
    /// Code that will execute during the run stage before the app is started
    /// </summary>
    public virtual void PreStartup(PreStartupContext context)
    {
    }

    /// <summary>
    /// Logic to apply when buildpack is ran.
    /// Note that for <see cref="SupplyBuildpack"/> this will correspond to "bin/supply" lifecycle event, while for <see cref="FinalBuildpack"/> it will be invoked on "bin/finalize"
    /// </summary>
    protected abstract void Apply(BuildContext context);


    internal void DoPreStartup(PreStartupContext context)
    {
        // var appHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // if (EnvironmentInfo.IsWin)
        //     appHome = Path.Combine(appHome, "app");
        // var preStartupContext = new PreStartupContext()
        // {
        //     AppDirectory = (VariablePath)Environment.GetEnvironmentVariable("HOME"),
        //     DependenciesDirectory = (VariablePath)(Environment.GetEnvironmentVariable("DEPS_DIR") ?? "~/deps"),
        //     BuildpackIndex = index
        // };
        PreStartup(context);
        var profiled = context.AppDirectory / ".profile.d";
        InstallStartupEnvVars(profiled, context.BuildpackIndex, true);
    }
    protected abstract bool IsPreStartOverridden { get; }
    protected void DoApply(BuildContext context)
    {
        PrintHeader();
        Apply(context);
        
        // var buildpackDepsDir = Path.Combine(depsPath, index.ToString());
        FileSystemTasks.EnsureExistingDirectory(context.CurrentDependencyDirectory);
        var profiled = context.BuildDirectory / ".profile.d";
        FileSystemTasks.EnsureExistingDirectory(profiled);
        if (IsPreStartOverridden) 
        {
            // copy buildpack to deps dir so we can invoke it as part of startup

            var currentAppPath = (AbsolutePath)Environment.GetCommandLineArgs()[0];
            var buildpackFolder = ((AbsolutePath)Environment.GetCommandLineArgs()[0]).Parent ?? throw new Exception("Unable to determine buildpack directory");
            var buildpackFiles = Directory.EnumerateFiles(buildpackFolder)
                .Cast<AbsolutePath>()
                .Where(x => !Lifecycle.AllValues.Where(l => l != Lifecycle.PreStartup).Contains(x.NameWithoutExtension)) // copy over the prestart assembly
                .ToList();
            foreach(var file in buildpackFiles)
            {
                FileSystemTasks.CopyFile(file, context.CurrentDependencyDirectory / file.Name);
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
            InstallStartupEnvVars(profiled, context.BuildpackIndex, false);
            GetEnvScriptFile(profiled, context.BuildpackIndex, true); // causes empty env file to be created so it can (potentially) be populated with vars during prestart hook
        }
            
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
    protected void InstallStartupEnvVars(VariablePath profiled, int index, bool isPreStart)
    {
        var envScriptName = GetEnvScriptFile(profiled, index, isPreStart);
            
        if (EnvironmentalVariables.Any())
        {
            if (IsLinux)
            {
                var envVars = EnvironmentalVariables.Aggregate(new StringBuilder(), (sb,x) => sb.Append($"export {x.Key}={Escape(x.Value)}\n"));
                File.WriteAllText(envScriptName, $"#!/bin/bash\n{envVars}");
            }
            else
            {
                var envVars = EnvironmentalVariables.Aggregate(new StringBuilder(), (sb,x) => sb.Append($"SET {x.Key}={x.Value}\r\n"));
                File.WriteAllText(envScriptName,envVars.ToString());
            }
        }
            
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\\\"")}\"";
}