namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class Droplet : IDisposable
{
    internal Droplet(AbsolutePath droplet)
    {
        DropletDirectory = droplet;
    }

    public AbsolutePath DropletDirectory { get; set; }
    public AbsolutePath ApplicationDirectory => DropletDirectory / "app";
    public AbsolutePath DependenciesDirectory => DropletDirectory / "deps";
    public AbsolutePath LogsDirectory => DropletDirectory / "logs";
    public AbsolutePath ProfiledDirectory => DropletDirectory / "profile.d";
    public AbsolutePath TempDirectory => DropletDirectory / "tmp";

    public void Dispose()
    {
        if (Directory.Exists(DropletDirectory))
        {
            try
            {
                DropletDirectory.DeleteDirectory();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}