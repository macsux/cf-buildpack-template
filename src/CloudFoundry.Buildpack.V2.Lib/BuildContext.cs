using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public sealed class BuildContext : BuildpackContext
{

    public VariablePath BuildDirectory { get; set; } = null!;
    /// <summary>
    /// Buildpack's `/lib` folder
    /// </summary>
    public VariablePath BuildpackLibDirectory { get; set; } = (VariablePath)(EntrypointExecutable.Parent.Parent / "lib").ToString();
    /// <summary>
    /// Folder where buildpacks contribute dependencies to. Each subfolder is index of buildpack that ran. Normally this folder is "~/deps" in container
    /// </summary>
    public VariablePath ContainerDependenciesDirectory { get; set; } = null!;
    public VariablePath CacheDirectory { get; set; } = null!;
    public int BuildpackIndex { get; set; }
    /// <summary>
    /// Dependency directory for CURRENT buildpack (deps/{index})
    /// </summary>
    public VariablePath TargetDependenciesDirectory => ContainerDependenciesDirectory / BuildpackIndex.ToString();
    public bool IsFinalize { get; set; }
    
    public AbsolutePath SourceDependenciesDirectory { get; set; } = EntrypointExecutable.Parent.Parent / "dependencies";
    public AbsolutePath GetDependencyDirectory(string name) => Directory.Exists(SourceDependenciesDirectory / name) ? SourceDependenciesDirectory / name : SourceDependenciesDirectory / BitConverter.ToString(_md5.ComputeHash(Encoding.ASCII.GetBytes(name))).ToLower().Replace("-", "");
    static MD5 _md5 = MD5.Create();

    //todo: finish this method
    private void InstallDependency(string name, string targetDir)
    {
        if (!Path.IsPathRooted(targetDir))
        {
            targetDir = TargetDependenciesDirectory / targetDir;
        }

        var sourceDir = GetDependencyDirectory(name);
        var files = Directory.EnumerateFiles(sourceDir).ToList();
        if (files.Count == 1) // archive - need to unpack
        {
            var archive = files.First();
            if (archive.EndsWith("tar.gz"))
            {
                var output = ProcessHelper.Execute($"tar -xf {archive} -C {targetDir}");
                Console.WriteLine(output.StdOut);
                Console.Error.WriteLine(output.ErrOut);
            }
        }
        else
        {
            FileSystemTasks.CopyDirectoryRecursively(sourceDir, targetDir);
        }
         
    }
}