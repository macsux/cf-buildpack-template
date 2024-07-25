using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using JetBrains.Annotations;
// using Newtonsoft.Json.Linq;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public partial class PluginInjectorBuildpack : SupplyBuildpack
{
	protected override void Apply(BuildContext context)
	{
		InstallNugetCache(context);
		InstallHttpModules(context);
		InstallHostStartups(context);

	}

	[UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Only strings are used with JsonArray.Add method")]
	public override void PreStartup(PreStartupContext context)
	{
		    //Console.WriteLine(new StackTrace().ToString());
	    Console.WriteLine("Adjusting runtimeconfig.json to probe nuget folder when resolving dependencies");
	    // if it was compiled from source, the app ends up in final buildpacks deps dir under dotnet_publish subfolder. in all other cases it's in it's regular spot: /home/vcap/app
	    AbsolutePath publishDir = (AbsolutePath)"/home/vcap/app"; 
	    if(Directory.Exists("/home/vcap/deps"))
	        publishDir = (AbsolutePath)Directory.EnumerateDirectories("/home/vcap/deps").OrderBy(x => x).Last() / "dotnet_publish";
	    if (!Directory.Exists(publishDir))
	        publishDir = (AbsolutePath)"/home/vcap/app";

	    foreach (var runtimeConfigFile in Directory.EnumerateFiles(publishDir, "*.runtimeconfig.json"))
	    {
	        // add path to nuget folder as part of "additionalProbingPaths" in runtimeconfig.json so extra assemblies we introduce from plugins can be resolved from there
	        var runtimeConfig = JsonNode.Parse(File.ReadAllText(runtimeConfigFile))!;
	        var runtimeOptions = runtimeConfig["runtimeOptions"]!;
	        var probingPaths = runtimeOptions["additionalProbingPaths"]?.AsArray();
	        if (probingPaths == null)
	        {
	            probingPaths = new JsonArray();
	            runtimeOptions["additionalProbingPaths"] = probingPaths;
	        }
	        
	        var nugetPackageDir = "/home/vcap/app/.nuget/packages";
	        if (!probingPaths.Select(x => x!.ToString()).Any(x => x == nugetPackageDir))
	        {
	            probingPaths.Add(JsonValue.Create(nugetPackageDir));
	        }
	        File.WriteAllText(runtimeConfigFile, runtimeConfig.ToString());
	    }
	    
	    var additionalDeps = Environment.GetEnvironmentVariable("DOTNET_ADDITIONAL_DEPS")?.Split(';') ?? Array.Empty<string>();
	    // var additionalDeps = new[]{@"C:\projects\MyWebApi\bin\Debug\net8.0\MyWebApi.deps.json"};
	    //todo: determine primary deps file
	    foreach (var appDepsFile in Directory.EnumerateFiles(publishDir, "*.deps.json"))
	    {
	        JsonNode appDepsJson = JsonNode.Parse(File.ReadAllText(appDepsFile))!;
	        string runtimeTarget = appDepsJson["runtimeTarget"]!["name"]?.ToString()! ?? throw new Exception($"Unable to read runtimeTarget.name from {appDepsFile}");
	        var isTargetRidSpecific = runtimeTarget.Contains("/");
	        var mergedDeps = new JsonObject();

	        foreach (var additionalDepFile in additionalDeps)
	        {
	            JsonNode additionalDepJson = JsonNode.Parse(File.ReadAllText(additionalDepFile))!;
	            // if our additional deps targets something like .NETCoreApp,Version=v8.0 when primary app is targeting .NETCoreApp,Version=v8.0/linux-x64,
	            // make the secondary deps RID specific as part of the merge
	            //additionalDepJson.Dump();

	            if (isTargetRidSpecific && additionalDepJson["targets"]![runtimeTarget] == null)
	            {
	                var framework = runtimeTarget.Split('/')[0];
	                var frameworkProperty = additionalDepJson["targets"]![framework]!.AsObject();
	                additionalDepJson["targets"]!.AsObject().Remove(framework);
	                additionalDepJson["targets"]![runtimeTarget] = frameworkProperty;
	            }
	            // adjust all nodes under /libraries that do not have "path" set. set this to lowercase library name, which should follow nuget folder convention
	            var libraries = additionalDepJson["libraries"]!.AsObject();
	            var libsWithoutPath = libraries.AsEnumerable().Where(x => x.Value!["path"] == null);
	            
	            foreach (var lib in libsWithoutPath)
	            {
	                var libName = lib.Key;
	                libraries[libName]!["path"] = libName.ToLower();
	            }
	            mergedDeps.Merge(additionalDepJson);
	        }

	        mergedDeps.Merge(appDepsJson);
			File.WriteAllText(appDepsFile, mergedDeps.ToString());
		}

		
	}

	void InstallHostStartups(BuildContext context)
	{
		var hostStartupPath = context.BuildpackRoot.LibDirectory / ".hostStartup";
		var hostStartupTypes = File.Exists(hostStartupPath) ? File.ReadAllLines(hostStartupPath) : Array.Empty<string>();
		var fqnRegex = new Regex(@"^(?<TypeName>[^,]+),\s?(?<AssemblyName>.+)\s*$");
		var hostStartupAssemblies = hostStartupTypes
			.Select(x => fqnRegex.Match(x))
			.Where(x => x.Success)
			.Select(x => x.Groups["AssemblyName"])
			.Distinct()
			.ToList();
		
		EnvironmentalVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = string.Join(";", hostStartupAssemblies);

		var depsFiles = Directory.EnumerateFiles(context.BuildpackRoot.LibDirectory, "*.deps.json").Select(x => (AbsolutePath)x).ToList();
		foreach (var file in depsFiles)
		{
			FileSystemTasks.CopyFile(file, context.MyDependenciesDirectory / file.Name);
		}
		EnvironmentalVariables["DOTNET_ADDITIONAL_DEPS"] = string.Join(";", depsFiles.Select(x => (AbsolutePath)"/home/vcap/deps" / context.BuildpackIndex.ToString() / x.Name));
		

	}
	void InstallHttpModules(BuildContext context)
	{
		var webConfigPath = context.BuildDirectory / "Web.config";
		if (!File.Exists(webConfigPath)) return;
		using var webConfig = new WebConfig(webConfigPath);
		var httpModulesFile = context.BuildpackRoot.LibDirectory / ".httpModules";
		var httpModules = File.Exists(httpModulesFile) ? File.ReadAllLines(httpModulesFile) : Array.Empty<string>();
		
		foreach (var httpModule in httpModules)
		{
			webConfig.AddHttpModule(httpModule);
			Console.WriteLine($"Registered {httpModule} HTTP module into web.config");
		}
	}
	void InstallNugetCache(BuildContext context)
	{
		var nugetSourceDir = context.BuildpackRoot.LibDirectory / ".nuget";
		var nugetTargetDir = context.BuildDirectory / ".nuget";
		FileSystemTasks.CopyDirectoryRecursively(nugetSourceDir, nugetTargetDir, DirectoryExistsPolicy.Merge, FileExistsPolicy.Skip);
		var webConfigPath = context.BuildDirectory / "Web.config";
		if (!File.Exists(webConfigPath)) return;
		using var webConfig = new WebConfig(webConfigPath);
		webConfig.CreateAssemblyBindings(nugetTargetDir);

		// var runtimeConfigFile = Directory.EnumerateFiles(context.BuildDirectory, "*.runtimeconfig.json").FirstOrDefault();
		// if (runtimeConfigFile != null && File.Exists(runtimeConfigFile))
		// {
		// 	var runtimeConfig = JObject.Parse(File.ReadAllText(runtimeConfigFile));
		// 	var runtimeOptions = (JObject)runtimeConfig["runtimeOptions"]!;
		// 	var probingPathsProperty = runtimeConfig["additionalProbingPaths"];
		// 	var probingPaths = probingPathsProperty?.Value<JArray>() ?? new();
		// 	if (probingPathsProperty == null)
		// 	{
		// 		probingPathsProperty = new JProperty("additionalProbingPaths", probingPaths);
		// 		runtimeOptions.Add(probingPathsProperty);
		// 	}
		// 	probingPaths.Add(new JValue("/home/vcap/app/.nuget/packages"));
		// }
	}
}

	

