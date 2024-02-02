using System;

if (args.Length != 4 || !int.TryParse(args[3], out var index))
{
    return PrintHelp();
}
var buildPath = args[0];
var cachePath = args[1];
var depsPath = args[2];

var commands = new MyBuildpack.Commands();
commands.Supply(buildPath, cachePath, depsPath, index);
return 0;

static int PrintHelp()
{
    Console.WriteLine("Usage: supply <buildpath> <cachePath> <depsPath> <index>");
    return -1;
}
