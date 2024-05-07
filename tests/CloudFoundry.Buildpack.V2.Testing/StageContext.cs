using System.Runtime.CompilerServices;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class StageContext : CloudFoundryContainerContext
{
    public AbsolutePath? CacheDirectory { get; set; }
    public AbsolutePath? DropletDirectory { get; set; }
    public List<AbsolutePath> Buildpacks { get; set; } = [];
    /// <summary>
    /// Skip detection stage and apply all buildpacks. This is the only way to do "multi-buildpack" execution, otherwise the first detected buildpack will be the one to execute
    /// </summary>
    public bool SkipDetect { get; set; }
    
}