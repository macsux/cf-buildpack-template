namespace CloudFoundry.Buildpack.V2.MyBuildpack;

#if(IsSupplyBuildpack)
public partial class MyBuildpackBuildpack : SupplyBuildpack
#elif(IsFinalBuildpack)
public partial class MyBuildpackBuildpack : FinalBuildpack
#elif(IsHttpModuleBuildpack || IsHostedServiceBuildpack)
public partial class MyBuildpackBuildpack : PluginInjectorBuildpack
#endif
{
#if(!IsHttpModuleBuildpack && !IsHostedServiceBuildpack)
#if(IsFinalBuildpack)
    public override bool Detect(DetectContext context)
    {
        return Directory.EnumerateFiles(context.BuildDirectory, "*.runtimeconfig.json").Any();
    }

    /// <summary>
    /// Sets the launch command for the container
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override string GetStartupCommand(ReleaseContext context)
    {
        var extension = Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : "";
        return $"SampleApp{extension}";
    }
#endif
    protected override void Apply(BuildContext context)
    {
        Console.WriteLine("Hello world");
        Console.WriteLine(context.SourceDependenciesDirectory);
        File.WriteAllText(context.BuildDirectory / "contrib.txt", "test");
        EnvironmentalVariables["MY_SETTING"] = "value"; // set any environmental variables for the app (staging phase)
#if(IsFinalBuildpack)
        EnvironmentalVariables["ASPNETCORE_HTTP_PORTS"] = "8080"; // set any environmental variables for the app (staging phase)
#endif
    }

    // uncomment below to install a hook that will run code before app starts running during "launch" phase of the app lifecycle 
    
    // public override void PreStartup(PreStartupContext context)
    // {
    //     Console.WriteLine("Application is about to start...");
    //     EnvironmentalVariables["MY_SETTING"] = "value"; // can set env vars before app starts running
    // }
#else
    public override void PreStartup(PreStartupContext context)
    {
        base.PreStartup(context);
    }
#endif
}