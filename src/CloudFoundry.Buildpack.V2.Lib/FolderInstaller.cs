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

    protected void ExtractPackage(AbsolutePath sourceDir)
    {
        var files = Directory.EnumerateFiles(sourceDir).ToList();
        if (files.Count == 1) // archive - need to unpack
        {
            var archive = files.First();
            (string StdOut, string ErrOut) output;
            if (archive.EndsWith(".tar.gz") || archive.EndsWith(".tar.xz") || archive.EndsWith(".tgz"))
            {
                output = ProcessHelper.Execute("/usr/bin/tar", arguments: $"-xf {archive} -C {sourceDir}");
                Console.WriteLine(output.StdOut);
                Console.Error.WriteLine(output.ErrOut);
                FileSystemTasks.DeleteFile(archive);
            }


            if (archive.EndsWith(".zip"))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(archive, sourceDir);
                FileSystemTasks.DeleteFile(archive);
            }

        }
    }
}
[PublicAPI]
public class FolderInstaller(BuildContext context) : Installer(context)
{
    public bool LogInstall { get; set; } = true;
    public void Install(DependencyPackage package, SemVersionRange versionRange, VariablePath? targetFolder = null)
    {
        var specificPackage = package.SelectVersion(versionRange) ?? throw new InvalidOperationException($"No packages of {package.Name} have version that satisfy version range {versionRange}");
        Install(specificPackage, targetFolder);
    }

    public VariablePath Install(DependencyVersion package, 
        VariablePath? targetFolder = null, 
        Func<RelativePath, bool>? excludePath = null)
    {
        
        targetFolder ??= Context.MyDependenciesDirectory / package.Name;
        if (LogInstall)
        {
            Console.WriteLine($"-----> Installing {package.Name} version {package.Version}");
            Console.WriteLine($"Copy package content {package.Directory} to {targetFolder}");
        }

        ExtractPackage(package.Directory.CurrentAbsolutePath);

        bool ExcludeFile(FileInfo info) => ExcludePath((AbsolutePath)info.FullName);
        bool ExcludeDirectory(DirectoryInfo info) => ExcludePath((AbsolutePath)info.FullName);
        bool ExcludePath(AbsolutePath directoryFullPath)
        {
            var relativePath = package.Directory.CurrentAbsolutePath.GetRelativePathTo(directoryFullPath);
            bool shouldExcludeDirectory = excludePath?.Invoke(relativePath) ?? false;
            return shouldExcludeDirectory;
        }
        FileSystemTasks.CopyDirectoryRecursively(package.Directory.CurrentAbsolutePath, targetFolder.CurrentAbsolutePath, excludeDirectory: ExcludeDirectory, excludeFile: ExcludeFile);
        return targetFolder;
    }
}