namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class LaunchContext : CloudFoundryContainerContext
{
    internal AbsolutePath? RootDirectory { get; set; }
    public AbsolutePath? DropletDirectory { get; }
    public override AbsolutePath ApplicationDirectory => DropletDirectory / "app";
    public AbsolutePath DependenciesDirectory => DropletDirectory / "deps";
    public AbsolutePath ProfileDDirectory => DropletDirectory / "profile.d";
    internal LaunchContext(AbsolutePath dropletDirectory, CloudFoundryStack stack)
    {
        RootDirectory = DirectoryHelper.RootDirectory;
        LifecycleDirectory = RootDirectory / "lifecycle";
        DropletDirectory = dropletDirectory;
        Stack = stack;
    }

    public static LaunchContext FromDropletDirectory(AbsolutePath dropletDirectory, CloudFoundryStack stack) => new (dropletDirectory, stack);
}