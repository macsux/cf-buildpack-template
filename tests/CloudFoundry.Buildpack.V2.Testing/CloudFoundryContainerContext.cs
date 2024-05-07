using CloudFoundry.Buildpack.V2.Testing.Models;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class CloudFoundryContainerContext
{
    public CloudFoundryContainerContext()
    {
        VcapApplication = new()
        {
            ApplicationName = "my-app",
            ApplicationId = Guid.NewGuid().ToString(),
            ApplicationUris = { "http://localhost:8080" },
            ApplicationVersion = Guid.NewGuid().ToString(),
            CfApi = "http://api.cf.local",
            Limits = new()
            {
                Disk = 1024,
                Fds = 100000,
                Mem = 1024
            },
            Name = "my-app",
            OrganizationId = Guid.NewGuid().ToString(),
            OrganizationName = "my-org",
            SpaceId = Guid.NewGuid().ToString(),
            SpaceName = "my-space",
            Uris = { "http://localhost:8080" },
            Version = Guid.NewGuid().ToString()
        };
        var packageDir = NuGetPackageResolver.GetGlobalInstalledPackage("CloudFoundry.Buildpack.V2.Lifecycle", version: null, packagesConfigFile: null)?.Directory;
        if (packageDir == null)
        {
            throw new InvalidOperationException(
                "Unable to locate lifecycle binaries. Please ensure you have added a <PackageDownload Include=\"CloudFoundry.Buildpack.V2.Lifecycle\" Version=\"[3.6.133]\" /> to csproj");
        }
        LifecycleDirectory = packageDir / "tools";
    }

    public Dictionary<string, string> EnvironmentalVariables { get; init; } = new();
    public VcapApplication VcapApplication { get; init; } = new();
    public VcapServices VcapServices { get; init; } = new();
    internal AbsolutePath? LifecycleDirectory { get; set; }
    public virtual AbsolutePath? ApplicationDirectory { get; init; }
    public CloudFoundryStack Stack { get; set; }


}