using System.Diagnostics;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

public static class ProcessHelper
{
    public static (string StdOut, string ErrOut) Execute(string filename, AbsolutePath? workingDirectory = null, string? arguments = null, Dictionary<string,string>? environmentalVariables = null)
    {
        if (workingDirectory == null)
        {
            workingDirectory = (AbsolutePath)Directory.GetCurrentDirectory();
        }

        var info = new ProcessStartInfo(filename)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = arguments ?? "",
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
        };
        if (environmentalVariables != null)
        {
            foreach (var env in environmentalVariables)
            {
                info.Environment.Add(env.Key, env.Value);
            }
        }

        var process = new Process();
        process.StartInfo = info;
        
        
        process.Start();
        process.WaitForExit();
        var std = process.StandardOutput.ReadToEnd();
        var err = process.StandardError.ReadToEnd();
        return (std, err);
    }
}