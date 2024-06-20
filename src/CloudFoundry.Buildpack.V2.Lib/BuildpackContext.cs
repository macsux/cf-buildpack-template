using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NMica.Utils;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

public class BuildpackContext
{
    internal static AbsolutePath EntrypointExecutable
    {
        get
        {
            var args = EnvironmentHelper.GetCommandLineArgsNative();
            //Console.WriteLine(string.Join(" ", args));
            // var args = Environment.GetCommandLineArgs().ToList().ToArray();
            if (EnvironmentInfo.IsWin && args[0].StartsWith(@"\"))
            {
                args[0] = $"c:{args[0]}";
            }
            if(!Path.IsPathRooted(args[0]))
            {
                args[0] = (AbsolutePath)Directory.GetCurrentDirectory() / args[0];
            }
            var entrypointExecutable = (AbsolutePath)args[0];
            return entrypointExecutable;
        }
    }

    
}