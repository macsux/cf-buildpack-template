using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using NMica.Utils.IO;
using Semver;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public sealed class BuildContext : BuildpackContext
{
    /// <summary>
    /// Provides information about currently executing buildpack
    /// </summary>
    public BuildpackRoot BuildpackRoot => BuildpackRoot.Instance;
    /// <summary>
    /// Application code directory. This is what was "pushed"
    /// </summary>
    public VariablePath BuildDirectory { get; set; } = null!;
    // /// <summary>
    // /// Buildpack's `/lib` folder
    // /// </summary>
    // public VariablePath BuildpackLibDirectory { get; set; } = (VariablePath)(BuildpackDirectory / "lib").ToString();
    /// <summary>
    /// Folder where buildpacks contribute dependencies to. Each subfolder is index of buildpack that ran. Normally this folder is "~/deps" in container
    /// </summary>
    public VariablePath DependenciesDirectory { get; set; } = null!;
    public VariablePath CacheDirectory { get; set; } = null!;
    public int BuildpackIndex { get; set; }
    /// <summary>
    /// Directory in which the CURRENT buildpack is expected to contribute dependencies to (deps/{index})
    /// </summary>
    public VariablePath MyDependenciesDirectory => DependenciesDirectory / BuildpackIndex.ToString();
    /// <summary>
    /// Indicates that this is the last stage in container creation - nothing else will contribute dependencies after this
    /// </summary>
    public bool IsFinalize { get; set; }
    
    public VariablePath InstallDependency(DependencyPackage package) =>
        InstallDependency(package.SelectVersion(SemVersionRange.AllRelease) ?? throw new InvalidOperationException($"No release versions are available for package {package.Name}"));
    public VariablePath InstallDependency(DependencyVersion package)
    {
        var installer = new FolderInstaller(this);
        return installer.Install(package);
    }
}