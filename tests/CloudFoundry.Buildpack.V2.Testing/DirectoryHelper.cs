using System.Reflection;
using NMica.Utils.Collections;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2.Testing;

public class DirectoryHelper
{
    static Lazy<AbsolutePath> _rootDirectory = new(() => (AbsolutePath)new DirectoryInfo(Directory.GetCurrentDirectory())
        .DescendantsAndSelf(x => x.Parent!)
        .FirstOrDefault(x => x.GetDirectories(".nuke").Any())
        ?.FullName);
    public static AbsolutePath RootDirectory => _rootDirectory.Value;
    
    static internal Lazy<AbsolutePath> _currentAssemblyFolder = new(() => ((AbsolutePath)Assembly.GetExecutingAssembly().Location).Parent);
    static internal AbsolutePath CurrentAssemblyFolder => _currentAssemblyFolder.Value;
}