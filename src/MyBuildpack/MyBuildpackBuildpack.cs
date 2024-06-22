namespace CloudFoundry.Buildpack.V2.MyBuildpack;

#if(IsSupplyBuildpack)
public partial class MyBuildpackBuildpack : SupplyBuildpack
#elif(IsHttpModuleBuildpack)
public partial class MyBuildpackBuildpack : HttpModuleBuildpack
#elif(IsHostedServiceBuildpack)
public partial class MyBuildpackBuildpack : HostedServiceBuildpack
#else
public partial class MyBuildpackBuildpack : FinalBuildpack
#endif
{
#if(!IsHttpModuleBuildpack && !IsHostedServiceBuildpack)
#if(!IsSupplyBuildpack)
    public override bool Detect(DetectContext context)
    {
        return true;
    }

    /// <summary>
    /// Sets the launch command for the container
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override string GetStartupCommand(ReleaseContext context)
    {
        return "test.exe";
    }
#endif
    protected override void Apply(BuildContext context)
    {
        Console.WriteLine("Hello world");
        Console.WriteLine(context.SourceDependenciesDirectory);
        File.WriteAllText(context.BuildDirectory / "contrib.txt", "test");
        EnvironmentalVariables["MY_SETTING"] = "value"; // set any environmental variables for the app (staging phase)
    }

    // uncomment below to install a hook that will run code before app starts running during "launch" phase of the app lifecycle 
    
    // public override void PreStartup(PreStartupContext context)
    // {
    //     Console.WriteLine("Application is about to start...");
    //     EnvironmentalVariables["MY_SETTING"] = "value"; // can set env vars before app starts running
    // }
#endif
}