using JetBrains.Annotations;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class BuildpackRoot : VariablePath
{
    internal const string DependenciesDirectoryName = "dependencies";
    internal const string LibrariesDirectoryName = "lib";
    internal const string BinDirectoryName = "bin";
    internal static BuildpackRoot Instance { get; } = new();
    internal BuildpackRoot() : base("%BUILDPACK_ROOT%") {}
    
    /// <summary>
    /// Location where the root of the currently executing buildpack is inside the container
    /// </summary>
    public VariablePath Directory { get; internal set; } = EnvironmentHelper.BuildpackDirectory;
    /// <summary>
    /// Buildpack's `/lib` folder
    /// </summary>
    public VariablePath LibDirectory => Directory / LibrariesDirectoryName;
    public VariablePath BinDirectory => Directory / BinDirectoryName;
    /// <summary>
    /// Buildpack's `/dependencies` folder
    /// </summary>
    public VariablePath DependenciesDirectory => Directory / DependenciesDirectoryName;

    public Dictionary<string, DependencyPackage> Dependencies { get; } = new();

}


