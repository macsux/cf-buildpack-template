using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
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

	public override void PreStartup(PreStartupContext context)
	{
		//Console.WriteLine(new StackTrace().ToString());
		Console.WriteLine($"PID: {Process.GetCurrentProcess().Id}");
		Console.WriteLine("Adjusting runtimeconfig.json to probe nuget folder when resolving dependencies");
		var publishDir = (AbsolutePath)Directory.EnumerateDirectories("/home/vcap/deps").OrderBy(x => x).Last() / "dotnet_publish";
		foreach (var runtimeConfigFile in Directory.EnumerateFiles(publishDir, "*.runtimeconfig.json"))
		{

			var runtimeConfig = JObject.Parse(File.ReadAllText(runtimeConfigFile));
			var runtimeOptions = (JObject)runtimeConfig["runtimeOptions"]!;
			var probingPathsProperty = runtimeOptions["additionalProbingPaths"];
			var probingPaths = probingPathsProperty?.Value<JArray>() ?? new();
			if (probingPathsProperty == null)
			{
				probingPathsProperty = new JProperty("additionalProbingPaths", probingPaths);
				runtimeOptions.Add(probingPathsProperty);
			}
			//  
			var nugetPackageDir = new JValue("/home/vcap/app/.nuget/packages");
			if (!probingPaths.ToList().Contains(nugetPackageDir))
			{
				probingPaths.Add(nugetPackageDir);
			}

			File.WriteAllText(runtimeConfigFile, runtimeConfig.ToString());
		}

		var additionalDeps = Environment.GetEnvironmentVariable("DOTNET_ADDITIONAL_DEPS")?.Split(';') ?? Array.Empty<string>();
		//todo: determine primary deps file
		foreach (var appDepsFile in Directory.EnumerateFiles(publishDir, "*.deps.json"))
		{
			var appDepsJson = JObject.Parse(File.ReadAllText(appDepsFile));
			string runtimeTarget = appDepsJson.SelectToken("runtimeTarget.name")?.ToString()! ?? throw new Exception($"Unable to read runtimeTarget.name from {appDepsFile}");
			var isTargetRidSpecific = runtimeTarget.Contains("/");
			var mergedDeps = new JObject();
			
			foreach (var additionalDepFile in additionalDeps)
			{
				var additionalDepJson = JObject.Parse(File.ReadAllText(additionalDepFile));
				if(isTargetRidSpecific && additionalDepJson.SelectToken($"targets.{runtimeTarget}") == null)
				{
					var framework = runtimeTarget.Split('/')[0];
					var frameworkProperty = (JProperty?)additionalDepJson.SelectToken($"targets.['{framework}']")?.Parent;
					frameworkProperty?.Replace(new JProperty(runtimeTarget, frameworkProperty.Value));
				}
				
				mergedDeps.Merge(additionalDepJson);
			}
			
			mergedDeps.Merge(appDepsJson); // merge primary deps last so it takes precedence over all supplimentary ones
			File.WriteAllText(appDepsFile, mergedDeps.ToString());
		}

		
	}

	void InstallHostStartups(BuildContext context)
	{
		var hostStartupPath = context.BuildpackLibDirectory / ".hostStartup";
		var hostStartupTypes = File.Exists(hostStartupPath) ? File.ReadAllLines(hostStartupPath) : Array.Empty<string>();
		var fqnRegex = new Regex(@"^(?<TypeName>[^,]+),\s?(?<AssemblyName>.+)\s*$");
		var hostStartupAssemblies = hostStartupTypes
			.Select(x => fqnRegex.Match(x))
			.Where(x => x.Success)
			.Select(x => x.Groups["AssemblyName"])
			.Distinct()
			.ToList();
		
		EnvironmentalVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = string.Join(";", hostStartupAssemblies);

		var depsFiles = Directory.EnumerateFiles(context.BuildpackLibDirectory, "*.deps.json").Select(x => (AbsolutePath)x).ToList();
		foreach (var file in depsFiles)
		{
			FileSystemTasks.CopyFile(file, context.TargetDependenciesDirectory / file.Name);
		}
		EnvironmentalVariables["DOTNET_ADDITIONAL_DEPS"] = string.Join(";", depsFiles.Select(x => (AbsolutePath)"/home/vcap/deps" / context.BuildpackIndex.ToString() / x.Name));
		

	}
	void InstallHttpModules(BuildContext context)
	{
		var webConfigPath = context.BuildDirectory / "Web.config";
		if (!File.Exists(webConfigPath)) return;
		using var webConfig = new WebConfig(webConfigPath);
		var httpModulesFile = context.BuildpackLibDirectory / ".httpModules";
		var httpModules = File.Exists(httpModulesFile) ? File.ReadAllLines(httpModulesFile) : Array.Empty<string>();
		
		foreach (var httpModule in httpModules)
		{
			webConfig.AddHttpModule(httpModule);
			Console.WriteLine($"Registered {httpModule} HTTP module into web.config");
		}
	}
	void InstallNugetCache(BuildContext context)
	{
		var nugetSourceDir = context.BuildpackLibDirectory / ".nuget";
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

	

