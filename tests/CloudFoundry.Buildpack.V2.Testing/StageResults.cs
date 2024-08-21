using System.IO.Compression;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Volumes;
using Xunit.Sdk;

namespace CloudFoundry.Buildpack.V2.Testing;

public class StageResults : IDisposable
{
    readonly ContainersPlatformFixture _fixture;

    internal StageResults(ContainersPlatformFixture fixture, IVolume dropletVolume, AbsolutePath dropletDirectory, (string StdOut, string ErrOut) logs)
    {
        _fixture = fixture;
        DropletVolume = dropletVolume;
        DropletDirectory = dropletDirectory;
    }

    internal IVolume DropletVolume { get; set; }
    internal List<string> Buildpacks { get; set; } = new();
    internal AbsolutePath DropletDirectory { get; set; }

    internal CloudFoundryStack Stack { get; set; }

    // public AbsolutePath GetDependencyDirectory(string buildpackName)
    // {
    //     var index = Buildpacks.IndexOf(buildpackName);
    //     return DependenciesDirectory / index.ToString();
    // }
    // public string StdOut { get; internal set; }
    // public string ErrOut { get; internal set; }
    public (string StdOut, string ErrOut) Logs { get; internal set; }

    public void Dispose()
    {
        // FileSystemTasks.DeleteDirectory(DropletDirectory);
    }

    public LaunchContext ToLaunchContext() => new(DropletVolume, Stack);

    public async Task<Droplet> GetDroplet(CancellationToken cancellationToken = default) => await GetDroplet(DropletDirectory, cancellationToken);
    public async Task<Droplet> GetDroplet(AbsolutePath localPath, CancellationToken cancellationToken = default)
    {
        var droplet = await _fixture.GetDroplet(DropletVolume, localPath, true, cancellationToken);
        TestContext.TestOutputHelper?.WriteLine($"Droplet downloaded to {localPath}");
        return droplet;
    }
}