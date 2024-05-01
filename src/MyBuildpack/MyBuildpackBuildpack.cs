namespace CloudFoundry.Buildpack.V2.MyBuildpack;

#if(IsSupplyBuildpack)
    public partial class MyBuildpackBuildpack : SupplyBuildpack
#else
public partial class MyBuildpackBuildpack : FinalBuildpack
#endif
{
#if(!IsSupplyBuildpack)
    public override bool Detect(DetectContext context)
    {
        return true;
    }
#endif
    protected override void Apply(BuildContext context)
    {
        Console.WriteLine("Hello world123");
        File.WriteAllText(context.BuildDirectory / "contrib.txt", "test");
        EnvironmentalVariables["MY_SETTING"] = "value"; // set any environmental variables for the app (staging phase)
    }
    
    public override void PreStartup(PreStartupContext context)
    {
        Console.WriteLine("Application is about to start...");
        EnvironmentalVariables["MY_SETTING"] = "value"; // can set env vars before app starts running
    }
#if(!IsSupplyBuildpack)
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
}