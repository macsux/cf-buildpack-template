using JetBrains.Annotations;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public sealed class PreStartupContext  : BuildpackContext
{
    public VariablePath AppDirectory { get; internal set; } = null!;
    public VariablePath DependenciesDirectory { get; internal set; } = null!;
    public int BuildpackIndex { get; set; }
    public VariablePath BuildpackDependencyDirectory => DependenciesDirectory / BuildpackIndex.ToString();
}