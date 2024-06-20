using JetBrains.Annotations;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public sealed class DetectContext : BuildpackContext
{
    public VariablePath BuildDirectory { get; internal set; } = null!;
}