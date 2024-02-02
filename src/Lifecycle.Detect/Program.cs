using System;

if (args.Length != 1)
{
    return PrintHelp();
}
var buildPath = args[0];
return new MyBuildpack.Commands().Detect(buildPath);

static int PrintHelp()
{
    Console.WriteLine("Usage: detect <buildpath>");
    return -1;
}
