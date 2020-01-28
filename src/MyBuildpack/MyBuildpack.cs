using System;
using System.IO;

namespace MyBuildpack
{
    public class MyBuildpack : FinalBuildpack //SupplyBuildpack 
    {
        protected override bool Detect(string buildPath)
        {
            return true;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            var myDependenciesDirectory = Path.Combine(depsPath, index.ToString()); // store any runtime dependencies not belonging to the app in this directory
            
            Console.WriteLine($"===Applying {nameof(MyBuildpack)}===");
            
            EnvironmentalVariables["MY_SETTING"] = "value"; // set any runtime environmental variables
            
        }
/*
        protected override void PreStartup(string buildPath, string depsPath, int index)
        {
            Console.WriteLine("Application is about to start...");
        }
*/
        public override string GetStartupCommand(string buildPath)
        {
            return "test.exe";
        }
    }
}
