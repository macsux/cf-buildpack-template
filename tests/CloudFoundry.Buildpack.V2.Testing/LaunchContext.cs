using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Volumes;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class LaunchContext : CloudFoundryContainerContext
{
    public IVolume DropletVolume { get; }
    public AbsolutePath? DropletDirectory { get; }
    public override AbsolutePath ApplicationDirectory => DropletDirectory / "app";
    public AbsolutePath DependenciesDirectory => DropletDirectory / "deps";
    public AbsolutePath ProfileDDirectory => DropletDirectory / "profile.d";
    public AbsolutePath TemporaryDirectory => DropletDirectory / "tmp";
    public Func<IWaitForContainerOS, IWaitForContainerOS> WaitStrategy { get; set; } = strategy => strategy.UntilPortIsAvailable(8080);

    internal LaunchContext(IVolume dropletVolume, CloudFoundryStack stack)
    {
        DropletVolume = dropletVolume;
        Stack = stack;
    }

    // public static LaunchContext FromDropletDirectory(AbsolutePath dropletDirectory, CloudFoundryStack stack) => new (dropletDirectory, stack);
}