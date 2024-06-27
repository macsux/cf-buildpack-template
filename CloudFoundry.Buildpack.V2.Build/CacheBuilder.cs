using System.Configuration;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using static Nuke.Common.IO.FileSystemTasks;
namespace CloudFoundry.Buildpack.V2.Build;

internal class CacheBuilder
{
    public static void CopyLibs(Project project, AbsolutePath libsDir)
    {
        // var project = Solution.GetProject("MyBuildpackModule") ?? throw new InvalidOperationException("Project MyBuildpackModule not found");
        var assemblyName = project.GetProperty("AssemblyName") ?? project.Name;
        var tfm = project.GetProperty("TargetFramework");
        var objFolder = project.Directory / "obj";
        // map side-by-side assembly loading from libs folder
        var assetsFile = objFolder / "project.assets.json";
	    var assetsDoc = JObject.Parse(File.ReadAllText(assetsFile));
        if (assetsDoc == null)
            throw new InvalidOperationException("project.assets.json is not found or invalid");
        var targetsNode = (JObject?)assetsDoc["targets"] ?? throw new InvalidOperationException($"targets node not found in {assetsFile}");
        var frameworkNode = targetsNode[tfm];

        if (frameworkNode == null)
        {
            var match = Regex.Match(tfm, @"(?<name>net|netstandard)(?<version>[0-9\.]+)");
            var frameworkVersion = match.Groups["version"].Value;
            if (!frameworkVersion.Contains(".")) //net472 vs net8.0 - ensure there are dots in version number
            {
                frameworkVersion = string.Join('.', frameworkVersion.ToArray());
            }
            string fullMonikerName;
            if (match.Groups["name"].Value == "net")
            {
                fullMonikerName = frameworkVersion.StartsWith("4") ? ".NETFramework" : ".NETCoreApp";
            }
            else if(match.Groups["name"].Value == "netstandard")
            {
                fullMonikerName = ".NETStandard";
            }
            else
            {
                throw new InvalidOperationException("Unsupported target framework");
            }

            frameworkNode = targetsNode
                .Properties()
                .FirstOrDefault(x => x.Name.StartsWith(fullMonikerName) && x.Name.Contains($"Version=v{frameworkVersion}"))
                ?.Value as JObject ?? throw new InvalidOperationException("Unable to locate target TFM in assets file");
        }
        
        var referenceAssemblies = frameworkNode!
		    .Cast<JProperty>()
		    .Where(x => x.Value["type"]?.ToString() == "package")
		    .SelectMany(item =>
		    {
			    var assemblyNameAndVersion = item.Name;
                var librariesNode = assetsDoc["libraries"] ?? throw new InvalidOperationException($"libraries node not found in {assetsFile}");
                var libraryNode = librariesNode[assemblyNameAndVersion] ?? throw new InvalidOperationException($"libraries/{assemblyNameAndVersion} node not found in {assetsFile}");
			    var srcFolder = libraryNode["path"]?.ToString() ?? throw new InvalidOperationException($"libraries/{assemblyNameAndVersion}/path node not found in {assetsFile}");
                return ((JObject?)item.Value["runtime"])?
                    .Properties()
                    .Select(x => Path.Combine(srcFolder, x.Name).Replace('/', Path.DirectorySeparatorChar)) ?? Enumerable.Empty<string>();            })
            .ToList();
        // var projectDlls = frameworkNode
        //     .Cast<JProperty>()
        //     .Where(x => x.Value["type"]?.ToString() == "project")
        //     .SelectMany(item => ((JObject?)item.Value["runtime"])?.Properties().Select(x => Path.GetFileName(x.Name))!)
        //     .Where(x => x != null)
        //     .ToList() ?? new();

        var userProfileDir = (AbsolutePath) Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	    var nugetCache = (AbsolutePath)Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? userProfileDir / ".nuget" / "packages";

        foreach (var file in referenceAssemblies)
        {
            CopyFile(nugetCache / file, libsDir / file, FileExistsPolicy.OverwriteIfNewer);
        }
        // var publishDir = project.Directory / "bin" / configuration / tfm ;
        // if (rid != null)
        // {
        //     publishDir /= rid;
        // }
        // projectAssembliesDir.CreateDirectory();
        // foreach (var projectDll in projectDlls)
        // {
        //     CopyFile(publishDir / projectDll, projectAssembliesDir / projectDll, FileExistsPolicy.OverwriteIfNewer);
        // }
        //
        // var assemblyFileName = $"{assemblyName}.dll";
        // // File.WriteAllText(projectAssembliesDir, assemblyFileName);
        // CopyFile(publishDir / assemblyFileName, projectAssembliesDir / assemblyFileName, FileExistsPolicy.OverwriteIfNewer);
        // return assemblyFileName;
    }
}