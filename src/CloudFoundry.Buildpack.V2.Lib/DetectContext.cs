using JetBrains.Annotations;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public sealed class DetectContext
{
    public VariablePath BuildDirectory { get; internal set; } = null!;
}

[PublicAPI]
public sealed class ReleaseContext
{
    public VariablePath BuildDirectory { get; internal set; } =  null!;

}

[PublicAPI]
public sealed class PreStartupContext 
{
    public VariablePath AppDirectory { get; internal set; } = null!;
    public VariablePath DependenciesDirectory { get; internal set; } = null!;
    public int BuildpackIndex { get; set; }
    public VariablePath BuildpackDependencyDirectory => DependenciesDirectory / BuildpackIndex.ToString();
}
[PublicAPI]
public sealed class BuildContext
{
    internal BuildContext()
    {
    }

    public VariablePath BuildDirectory { get; internal set; } = null!;
    public VariablePath DependenciesDirectory { get; internal set; } = null!;
    public VariablePath CacheDirectory { get; internal set; } = null!;
    public int BuildpackIndex { get; internal set; }
    public VariablePath CurrentDependencyDirectory => DependenciesDirectory / BuildpackIndex.ToString();
    public bool IsFinalize { get; set; }
}

