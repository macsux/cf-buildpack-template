using JetBrains.Annotations;
using NMica.Utils.IO;
using Semver;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public abstract class Installer
{
    public BuildContext Context { get; }

    protected Installer(BuildContext context)
    {
        Context = context;
    }
}
[PublicAPI]
public class FolderInstaller(BuildContext context) : Installer(context)
{
    public void Install(DependencyPackage package, SemVersionRange versionRange, VariablePath? targetFolder = null)
    {
        var specificPackage = package.SelectVersion(versionRange) ?? throw new InvalidOperationException($"No packages of {package.Name} have version that satisfy version range {versionRange}");
        Install(specificPackage, targetFolder);
    }

    public VariablePath Install(DependencyVersion package, VariablePath? targetFolder = null, bool logInstall = true)
    {
        
        targetFolder ??= Context.MyDependenciesDirectory / package.Name;
        if (logInstall)
        {
            Console.WriteLine($"-----> Installing {package.Name} version {package.Version} into {targetFolder}");
        }

        var allFiles = package.SelectFiles();
        foreach (var (absolutePath, relativePath) in allFiles)
        {
            FileSystemTasks.CopyFile(absolutePath, targetFolder / relativePath);
        }
        // bool ExcludeFile(FileInfo info) => ExcludePath((AbsolutePath)info.FullName);
        // bool ExcludeDirectory(DirectoryInfo info) => ExcludePath((AbsolutePath)info.FullName);
        // bool ExcludePath(AbsolutePath directoryFullPath)
        // {
        //     var relativePath = package.Directory.CurrentAbsolutePath.GetRelativePathTo(directoryFullPath);
        //     bool shouldExcludeDirectory = excludePath?.Invoke(relativePath) ?? false;
        //     return shouldExcludeDirectory;
        // }
        
        
        // FileSystemTasks.CopyDirectoryRecursively(package.Folder.CurrentAbsolutePath, targetFolder.CurrentAbsolutePath, excludeDirectory: ExcludeDirectory, excludeFile: ExcludeFile);
        return targetFolder;
    }
    
    
}