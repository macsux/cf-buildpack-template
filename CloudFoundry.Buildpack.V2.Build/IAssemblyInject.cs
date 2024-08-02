using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
namespace CloudFoundry.Buildpack.V2.Build;


public interface IAssemblyInject : IBuildpackBase
{
    static string? InjectionProject { get; set; }

    Target TryFindInjectionProject => _ => _
        .Unlisted()
        .Executes(() =>
        {
            var injectable = Solution.GetAllProjects("*")
                .FirstOrDefault(x => x.GetMSBuildProject(Configuration).GetProperty("IsInjectable")?.EvaluatedValue.ToLower() == "true");
            Log.Logger.Information("Injectable project: {Project}", injectable?.ToString() ?? "none");
            InjectionProject = injectable?.Name;
        });

    Target BuildInjectors => _ => _
        .Description("Packages assemblies referenced by project for side-by-side loading")
        .Unlisted()
        .DependsOn(EnsureCleanWorkDirectory, TryFindInjectionProject)
        .OnlyWhenDynamic(() => InjectionProject != null)
        // .Requires(() => InjectionProject)
        .Executes(() =>
        {
            var project = Solution.GetAllProjects(InjectionProject).FirstOrDefault() ?? throw new Exception($"Project {InjectionProject} is not found");
            var assemblyName = project.GetProperty("AssemblyName") ?? project.Name;
            var output = DotNetPack(s => s
                .SetProject(project.Path)
                .SetConfiguration(Configuration)
            );

            var packagePath = Directory.EnumerateFiles(project.Directory / "bin" / Configuration, $"{project.Name}*.nupkg").OrderBy(x => x).Last();
            
            foreach (var publishCombination in PublishCombinations)
            {
                

                // var publishCombination = PublishCombinations.First(x => x.Stack == StackType.Windows);
                var framework = publishCombination.Framework;
                var runtime = publishCombination.Runtime;
                var publishWorkDirectory = WorkDirectory / runtime;
                // var targetProject = "MyBuildpackHttpModule";
                var nugetStoreDir = publishWorkDirectory / "lib" / ".nuget" / "packages";

                
                
                // var packagePathRegex = new Regex("Successfully created package '(?<path>[^']+)");
                //
                // var packagePath = output
                //     .Select(x => packagePathRegex.Match(x.Text))
                //     .First(x => x.Success)
                //     .Groups["path"]
                //     .Value;
                NuGetTasks.NuGet($"add {packagePath} -Source {nugetStoreDir} -Expand"); // expand the package into nuget cache we're building

                CacheBuilder.CopyLibs(project, nugetStoreDir);


                var mainAssemblyPathRegex = new Regex($"{assemblyName} -> (?<path>.+\\.dll)");
                var mainAssemblyPath = (AbsolutePath)output
                    .Select(x => mainAssemblyPathRegex.Match(x.Text))
                    .First(x => x.Success)
                    .Groups["path"]
                    .Value;

                var dotnetSharedPath = ((AbsolutePath)RuntimeEnvironment.GetRuntimeDirectory()).Parent?.Parent;
                var latestNetCoreApp = Directory.EnumerateDirectories(dotnetSharedPath / "Microsoft.NETCore.App").OrderBy(x => x).Last();
                var latestAspCoreApp = Directory.EnumerateDirectories(dotnetSharedPath / "Microsoft.AspNetCore.App").OrderBy(x => x).Last();
                var paths = new List<string>();
                paths.AddRange(Directory.EnumerateFiles(latestNetCoreApp, "*.dll", SearchOption.AllDirectories));
                paths.AddRange(Directory.EnumerateFiles(latestAspCoreApp, "*.dll", SearchOption.AllDirectories));
                paths.Add(mainAssemblyPath);
                paths.Remove(paths.First(x => x.EndsWith("System.Web.dll")));
                paths.Add(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.dll");
                var resolver = new PathAssemblyResolver(paths);
                var mlc = new MetadataLoadContext(resolver);
                var assembly = mlc.LoadFromAssemblyPath(mainAssemblyPath);

                var httpModules = assembly.ExportedTypes
                    .Where(x => x.GetInterface("IHttpModule") != null)
                    .Select(x => x.AssemblyQualifiedName)
                    .ToList();
                if (httpModules.Any())
                {
                    File.WriteAllText(publishWorkDirectory / "lib" / ".httpmodules", string.Join("\n", httpModules));
                }

                var hostStartupModules = assembly.CustomAttributes
                    .Where(x => x.AttributeType.AssemblyQualifiedName == typeof(HostingStartupAttribute).AssemblyQualifiedName)
                    .Select(x => x.ConstructorArguments.First().Value)
                    .Cast<Type>()
                    .Select(x => x.AssemblyQualifiedName)
                    .ToList();

                if (!hostStartupModules.Any()) return;

                var depsFileName = $"{mainAssemblyPath.Name.TrimEnd(".dll")}.deps.json";
                var depsSourceFilePath = mainAssemblyPath.Parent / depsFileName;
                var depsTargetFilePath = publishWorkDirectory / "lib" / depsFileName;
                CopyFile(depsSourceFilePath, depsTargetFilePath);
                var depsJson = JObject.Parse(File.ReadAllText(depsTargetFilePath));
                File.WriteAllText(publishWorkDirectory / "lib" / ".hostStartup", string.Join("\n", hostStartupModules));

                SwitchDepsLibrariesToPackageType(project, depsTargetFilePath);
            }
        });

    /// <summary>
    /// Adjusts deps file to have all libraries be of type "package" instead of project so they can be resolved from nuget cache
    /// </summary>
    private void SwitchDepsLibrariesToPackageType(Project project, AbsolutePath depsTargetFilePath)
    {
        var depsJson = JObject.Parse(File.ReadAllText(depsTargetFilePath));
        var targetFramework = project.GetProperty<string>("TargetFramework");
        var libraryTypes = depsJson.SelectTokens("libraries.*.type").Where(x => x.Value<string>() == "project").ToList();
        foreach(var type in libraryTypes)
        {
            type.Replace(new JValue("package"));
        }

        var nonLibRuntimePaths = depsJson.SelectTokens("targets.*.*.runtime")
            .Cast<JObject>()
            .Select(x => x.Properties())
            .SelectMany(x => x)
            .Where(x => !x.Name.StartsWith("lib/"))

            // .Where(x => x.Name.StartsWith($"{project.Name}/"))
            .ToList();
        foreach(var dllPath in nonLibRuntimePaths)
        {
            dllPath.Replace(new JProperty($"lib/{targetFramework}/{dllPath.Name}"));
        }
        
        File.WriteAllText(depsTargetFilePath, depsJson.ToString());
    }


    
}