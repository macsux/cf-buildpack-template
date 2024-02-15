using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2.Testing;

public class StageResults : IDisposable
{
    public StageResults(AbsolutePath dropletDirectory, (string StdOut, string ErrOut) logs)
    {
        DropletDirectory = dropletDirectory;
        Logs = logs;
    }

    internal List<string> Buildpacks { get; set; } = new();
    internal AbsolutePath DropletDirectory { get; set; } = null!;
    public AbsolutePath ApplicationDirectory => DropletDirectory / "app";
    public AbsolutePath DependenciesDirectory => DropletDirectory / "deps";
    public AbsolutePath LogsDirectory => DropletDirectory / "logs";
    public AbsolutePath ProfiledDirectory => DropletDirectory / "profile.d";
    public AbsolutePath TempDirectory => DropletDirectory / "tmp";
    internal CloudFoundryStack Stack { get; set; }

    public AbsolutePath GetDependencyDirectory(string buildpackName)
    {
        var index = Buildpacks.IndexOf(buildpackName);
        return DependenciesDirectory / index.ToString();
    }
    // public string StdOut { get; internal set; }
    // public string ErrOut { get; internal set; }
    public (string StdOut, string ErrOut) Logs { get; internal set; }

    public void Dispose()
    {
        // FileSystemTasks.DeleteDirectory(DropletDirectory);
    }

    public LaunchContext ToLaunchContext() => new(DropletDirectory, Stack);
}