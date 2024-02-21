namespace CloudFoundry.Buildpack.V2.MyBuildpack;

public partial class MyBuildpack : FinalBuildpack //SupplyBuildpack 
{
    public override bool Detect(DetectContext context)
    {
        return true;
    }

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

    public override string GetStartupCommand(ReleaseContext buildPath)
    {
        return "test.exe";
    }
}