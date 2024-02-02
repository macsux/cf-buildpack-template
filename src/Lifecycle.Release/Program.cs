using System;

if (args.Length != 4 && !int.TryParse(args[3], out var index))
{
    return PrintHelp();
}
var buildPath = args[0];
var commands = new MyBuildpack.Commands();
commands.Release(buildPath);
return 0;
static int PrintHelp()
{
    Console.WriteLine("Usage: release <buildpath>");
    return -1;
}