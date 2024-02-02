using System;

if (args.Length != 1 || !int.TryParse(args[3], out var index))
{
    return PrintHelp();
}
var commands = new MyBuildpack.Commands();
commands.PreStartup(index);
return 0;
static int PrintHelp()
{
    Console.WriteLine("Usage: prestartup <buildpath>");
    return -1;
}