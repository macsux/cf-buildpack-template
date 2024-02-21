using JetBrains.Annotations;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class CloudFoundryContainerContext
{
    public AbsolutePath? LifecycleDirectory { get; set; }
    public virtual AbsolutePath? ApplicationDirectory { get; init; }
    public CloudFoundryStack Stack { get; set; }


}