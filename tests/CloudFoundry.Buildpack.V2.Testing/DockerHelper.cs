using System.Diagnostics;

namespace CloudFoundry.Buildpack.V2.Testing;

public class DockerHelper
{
    public static (string StdOut, string StdErr) SwitchContainersPlatform(ContainerPlatform platformFamily)
    {
        string stdOut = "";
        string stdErr = "";
        var arg = platformFamily switch
        {
            ContainerPlatform.Windows => "-SwitchWindowsEngine",
            ContainerPlatform.Linux => "-SwitchLinuxEngine",
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
            stdOut = process?.StandardOutput.ReadToEnd() ?? "";
            stdErr = process?.StandardError.ReadToEnd() ?? "";
            
        }
        return (stdOut, stdErr);
    } 
}