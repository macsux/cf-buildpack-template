using Microsoft.Extensions.Hosting;
using NMica.Utils.IO;
using Semver;
using Serilog;

namespace CloudFoundry.Buildpack.V2.MyBuildpack;

#if(IsSupplyBuildpack)
public partial class MyBuildpack : SupplyBuildpack
#elif(IsFinalBuildpack)
public partial class MyBuildpack : FinalBuildpack
#elif(IsHttpModuleBuildpack || IsHostedServiceBuildpack)
public partial class MyBuildpack : PluginInjectorBuildpack
#endif
{
#if(!IsHttpModuleBuildpack && !IsHostedServiceBuildpack)

    public override DetectResult Detect(DetectContext context)
    {
        var requiredDependencies = new List<IDependencyDemand>()
        {
            VersionDependencyDemand.Create("dotnet-sdk", SemVersionRange.All)
        };
        return DetectResult.Pass(requiredDependencies);
    }

    protected override BuildResult Apply(BuildContext context)
    {
        var buildResult = new BuildResult();
        Log.Information("Hello world");
        File.WriteAllText(context.BuildDirectory.CurrentAbsolutePath / "contrib.txt", "test");
        buildResult.EnvironmentalVariables.Set("MY_SETTING", "value"); // set any environmental variables for the app (staging phase)
#if(IsFinalBuildpack)
        buildResult.EnvironmentalVariables.Set("ASPNETCORE_HTTP_PORTS", "8080"); // set any environmental variables for the app (staging phase)
#endif
        return buildResult;
    }

#if(IsFinalBuildpack)
    /// <summary>
    /// Sets the launch command for the container
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override string GetStartupCommand(ReleaseContext context)
    {
        var extension = Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : "";
        return $"./SampleApp{extension}";
    }
#endif
    
    // uncomment below to install a hook that will run code before app starts running during "launch" phase of the app lifecycle 
    
    // public override PreStartResult PreStartup(PreStartupContext context)
    // {
    //     Console.WriteLine("Application is about to start...");
    //     var result = new PreStartResult();
    //     result.EnvironmentalVariables.Set("MY_SETTING", "value"); // can set env vars before app starts running
    //     return result;
    // }
#endif
}