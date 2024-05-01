using System.Reflection;
using NMica.Utils.Collections;

namespace CloudFoundry.Buildpack.V2.Testing;

public static class DirectoryHelper
{
    static readonly Lazy<AbsolutePath> _rootDirectory = new(() => (AbsolutePath)new DirectoryInfo(Directory.GetCurrentDirectory())
        .DescendantsAndSelf(x => x.Parent!)
        .FirstOrDefault(x => x.GetDirectories(".nuke").Any())
        ?.FullName);
    public static AbsolutePath RootDirectory => _rootDirectory.Value;
    
    static Lazy<AbsolutePath> _currentAssemblyFolder = new(() => ((AbsolutePath)Assembly.GetExecutingAssembly().Location).Parent);
    internal static AbsolutePath CurrentAssemblyFolder => _currentAssemblyFolder.Value;
}