using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NMica.Utils.Collections;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class StageContext : CloudFoundryContainerContext
{
    internal AbsolutePath? RootDirectory { get; set; }
    // public AbsolutePath? LifecycleDirectory { get; set; }
    public AbsolutePath? CacheDirectory { get; set; }
    // public AbsolutePath? ApplicationDirectory { get; set; }
    public AbsolutePath? DropletDirectory { get; set; }
    public List<AbsolutePath> Buildpacks { get; set; } = [];
    // public CloudFoundryStack Stack { get; set; }
    /// <summary>
    /// Skip detection stage and apply all buildpacks. This is the only way to do "multi-buildpack" execution, otherwise the first detected buildpack will be the one to execute
    /// </summary>
    public bool SkipDetect { get; set; }
    
    public static StageContext Generate(AbsolutePath appPath, CloudFoundryStack stack, [CallerMemberName]string callingMethod = "test")
    {
        var rootDir = (AbsolutePath)new DirectoryInfo(Directory.GetCurrentDirectory())
            .DescendantsAndSelf(x => x.Parent!)
            .FirstOrDefault(x => x.GetDirectories(".nuke").Any())
            ?.FullName;
        var baseDir = (AbsolutePath)Directory.GetCurrentDirectory() / $"{callingMethod}-{DateTime.Now.Ticks:x}";
        var context = new StageContext
        {
            RootDirectory = rootDir,
            Stack = stack,
            CacheDirectory = baseDir / "cache",
            ApplicationDirectory = appPath,
            DropletDirectory = baseDir / "droplet",
            LifecycleDirectory = rootDir / "lifecycle"
        };

        return context;
    }
}