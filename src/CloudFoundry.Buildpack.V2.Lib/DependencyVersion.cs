using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Semver;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class DependencyVersion
{
    static MD5 _md5 = MD5.Create();

    private static string ToMd5Hash(string name) => BitConverter.ToString(_md5.ComputeHash(Encoding.ASCII.GetBytes(name))).ToLower().Replace("-", "");

    internal DependencyVersion(string name, SemVersion version, DependencyVersion supersetPackage) : this(name,version, null, supersetPackage.Directory.CurrentAbsolutePath.Name)
    {
    }
    internal DependencyVersion(string name, SemVersion version, Uri? uri, string? folderName = null)
    {
        _folderName = folderName ?? ToMd5Hash(uri?.ToString() ?? $"{name}-{version}");
        Name = name;
        Version = version;
        Uri = uri;
    }

    string _folderName;
    public VariablePath Directory => BuildpackRoot.Instance.DependenciesDirectory / _folderName;
    public Uri? Uri { get; }
    public string Name { get; }
    public SemVersion Version { get; }
}