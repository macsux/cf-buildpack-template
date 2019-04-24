using System;

namespace MyBuildpack
{
    public class MyBuildpack : SupplyBuildpack
    {
        static int Main(string[] args)
        {
            return new MyBuildpack().Run(args);
        }
        protected override bool Detect(string buildPath)
        {
            return false;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            Console.WriteLine("Test");
        }
    }
}
