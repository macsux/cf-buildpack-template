using System;
using System.IO;

namespace cf_buildpack_template
{
    public class MyBuildpack : SupplyBuildpack
    {
        static void Main(string[] args)
        {
            new MyBuildpack().Run(args);
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

    public abstract class Buildpack
    {
        protected abstract bool Detect(string buildPath);

        protected abstract void Apply(string buildPath, string cachePath, string depsPath, int index);

        public int Run(string[] args)
        {
            return DoRun(args);
        }

        protected virtual int DoRun(string[] args)
        {
            var command = args[0];
            switch (command)
            {
                case "detect":
                    return Detect(args[1]) ? 0 : 1;
                    break;
                case "supply":
                    Apply(args[1], args[2], args[3], int.Parse(args[4]));
                    break;
            }

            return 0;
        }

    }

    public abstract class FinalBuildpack : Buildpack
    {
        public abstract string GetStartupCommand(string buildPath);
        protected override int DoRun(string[] args)
        {
            var command = args[0];
            switch (command)
            {
                case "supply":
                    return 0;
                case "finalize":

            }
            if (args[0] == "release")
            {
                var starupCommand = GetStartupCommand(args[1]);
                Console.WriteLine("default_process_types:");
                Console.WriteLine($"  web: {starupCommand}");
                return 0;
            }
            else
            {
                return base.DoRun(args);
            }
        }
    }

    public abstract class SupplyBuildpack : Buildpack
    {

    }
}
