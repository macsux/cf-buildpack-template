using System.Reflection;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;

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
    
    internal static AbsolutePath GetPackageDirectory(string packageId) => NuGetPackageResolver.GetGlobalInstalledPackage(packageId, null, null)?.Directory
                                                                          ?? throw new InvalidOperationException($"Package {packageId} not found in nuget cache");
    
}