using System.Security.Cryptography;
using System.Text;
using GlobExpressions;
using JetBrains.Annotations;
using NMica.Utils.IO;
using Semver;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class DependencyVersion
{
    static MD5 _md5 = MD5.Create();
    static string[] _wildcardGlob = ["**"];
    internal static string ToMd5Hash(string name) => BitConverter.ToString(_md5.ComputeHash(Encoding.ASCII.GetBytes(name))).ToLower().Replace("-", "");

    // internal DependencyVersion(string name, SemVersion version, IEnumerable<DependencyVersion> parts, IEnumerable<string>? include = null, IEnumerable<string>? exclude = null) 
    //     : this(name, version, (Uri?)null, include, exclude)
    // {
    //     
    //     Folder = null;
    // }
    internal DependencyVersion(string name, SemVersion version, Uri? uri = null, IEnumerable<DependencyVersion>? parts = null, IEnumerable<string>? include = null, IEnumerable<string>? exclude = null, VariablePath? dependenciesDirectory = null)
    {
        var folderName = ToMd5Hash(uri?.ToString() ?? $"{name}-{version}");
        Parts = parts?.ToArray() ?? [];
        Folder = !Parts.Any() ? (dependenciesDirectory ?? BuildpackRoot.Instance.DependenciesDirectory) / folderName : null;
        Name = name;
        Version = version;
        Uri = uri;
        Include = include?.ToArray() ?? _wildcardGlob;
        Exclude = exclude?.ToArray() ?? [];
    }

    public VariablePath? Folder { get; internal set; }
    public Uri? Uri { get; }
    public string Name { get; }
    public SemVersion Version { get; }

    /// <summary>
    /// Other dependencies which contains fragments from which this dependency version is composed of
    /// </summary>
    public IReadOnlyCollection<DependencyVersion> Parts { get; private set; } = [];
    public IReadOnlyCollection<string> Include { get; private set; }
    public IReadOnlyCollection<string> Exclude { get;  private set;}

    public IEnumerable<(AbsolutePath AbsolutePath, RelativePath RelativePath)> SelectFiles() => SelectFiles(null);
    internal IEnumerable<(AbsolutePath AbsolutePath, RelativePath RelativePath)> SelectFiles(string? partOfVersion)
    {
        Extract();

        string[] ReplaceTokens(IEnumerable<string> patterns) => patterns
            .Select(x => x
                .Replace("{version}", Version.ToString())
                .Replace("{partof-version}", partOfVersion))
            .ToArray();
        var include = ReplaceTokens(Include);
        var exclude = ReplaceTokens(Exclude);
        IEnumerable<(AbsolutePath, RelativePath)> ownFiles;
        if (Folder != null)
        {
            ownFiles = Directory.EnumerateFiles(Folder.CurrentAbsolutePath, "*", SearchOption.AllDirectories)
                .Select(x => "/" + Folder.CurrentAbsolutePath.GetRelativePathTo(x).ToUnixRelativePath())
                .Where(file => include.Any(pattern => Glob.IsMatch(file, pattern)))
                .Where(file => !exclude.Any(pattern => Glob.IsMatch(file, pattern)))
                .Select(localPath => (AbsolutePath: Folder.CurrentAbsolutePath / localPath.TrimStart('/'), RelativePath: (RelativePath)localPath.TrimStart('/')))
                .ToList();
        }
        else
        {
            ownFiles = Array.Empty<(AbsolutePath, RelativePath)>();
        }
        // var ownFiles = Folder != null ? Directory.EnumerateFiles(Folder, "", SearchOption.AllDirectories) : Array.Empty<string>();
        var packageFiles = ownFiles
            // .Select(x => "/" + Folder.CurrentAbsolutePath.GetRelativePathTo(x))
            // .Where(file => include.Any(pattern => Glob.IsMatch(file, pattern))) 
            // .Where(file => !exclude.Any(pattern => Glob.IsMatch(file, pattern))) 
            // .Select(localPath => (AbsolutePath: Folder.CurrentAbsolutePath / localPath.TrimStart('/'), RelativePath: (RelativePath)localPath.TrimStart('/')))
            .Union(Parts.SelectMany(x => x.SelectFiles(Version.ToString())));
        return packageFiles;
    }

    public DependencyVersion Slice(IEnumerable<string>? include = null, IEnumerable<string>? exclude = null)
    {
        var clone = (DependencyVersion)MemberwiseClone();
        clone.Parts = [];
        clone.Include = include?.ToArray() ?? [];
        clone.Exclude = exclude?.ToArray() ?? [];
        return clone;
    }
    
    protected void Extract()
    {
        var sourceDir = Folder?.CurrentAbsolutePath;
        if(Folder == null || !sourceDir.DirectoryExists())
            return;
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