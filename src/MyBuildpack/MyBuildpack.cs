using System;
using System.IO;

namespace CloudFoundry.Buildpack.V2.MyBuildpack;

public partial class MyBuildpack : FinalBuildpack //SupplyBuildpack 
{
    protected override string? BuildpackVersion => ThisAssembly.AssemblyVersion;

    public override bool Detect(DetectContext context)
    {
        return false;
    }

    protected override void Apply(BuildContext context)
    {
        Console.WriteLine($"===Applying {nameof(MyBuildpack)}===");
            
        EnvironmentalVariables["MY_SETTING"] = "value"; // set any environmental variables for the app (staging phase)
            
    }


    /*
        protected override void PreStartup(string buildPath, string depsPath, int index)
        {
            Console.WriteLine("Application is about to start...");
            EnvironmentalVariables["MY_SETTING"] = "value"; // can set env vars before app starts running
        }
*/
    public override string GetStartupCommand(ReleaseContext buildPath)
    {
        return "test.exe";
    }
}