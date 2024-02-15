using System.Diagnostics;
using NMica.Utils;
using NMica.Utils.IO;
using Xunit.Abstractions;

namespace CloudFoundry.Buildpack.V2.Testing;

public class DockerHelper
{
    public static void SwitchContainersPlatform(PlatformFamily platformFamily, ITestOutputHelper output = null)
    {
        var arg = platformFamily switch
        {
            PlatformFamily.Windows => "-SwitchWindowsEngine",
            PlatformFamily.Linux => "-SwitchLinuxEngine",
            _ => throw new ArgumentOutOfRangeException(nameof(platformFamily), "Only Windows or Linux options are supported")
        };
        if (EnvironmentInfo.IsWin)
        {
            var programFiles = (AbsolutePath)Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var dockerCli = programFiles / "Docker" / "Docker" / "DockerCli.exe";
            var processStart = new ProcessStartInfo
            {
                FileName = dockerCli,
                Arguments = arg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var process = Process.Start(processStart);
            var stdOut = process.StandardOutput.ReadToEnd();
            var stdErr = process.StandardError.ReadToEnd();
            output?.WriteLine(stdOut);
            output?.WriteLine(stdErr);
        }
    } 
}