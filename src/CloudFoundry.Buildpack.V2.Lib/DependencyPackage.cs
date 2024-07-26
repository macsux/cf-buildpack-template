using JetBrains.Annotations;
using Semver;

namespace CloudFoundry.Buildpack.V2;

//
// public class DependencyBuilder
// {
//     public DependencyBuilder(string name)
//     {
//     }
//
//     public DependencyVersionBuilder AddVersion(string version) => throw new NotImplementedException();
// }
//
// public class DependencyVersionBuilder
// {
//     public DependencyVersionBuilder AddVersion(string version) => throw new NotImplementedException();
//     public DependencyVersionBuilder WithUri(string uri) => throw new NotImplementedException();
//     public DependencyVersionBuilder ComposedFrom(string dependency, string version, IEnumerable<string>? includePatterns = null, IEnumerable<string>? excludePatterns = null) => throw new NotImplementedException();
// }



[PublicAPI]
public class DependencyPackage
{
    // public static DependencyBuilder Create(string name) => new DependencyBuilder(name);
    List<DependencyVersion> _versions = new();
    public string Name { get; }
    public DependencyPackage(string name)
    {
        Name = name;
    }

    public DependencyPackage AddVersion(SemVersion version, IEnumerable<DependencyVersion> parts)
    {
        var dependencyVersion = new DependencyVersion(Name, version, parts);
        _versions.Add(dependencyVersion);
        return this;
    }
    public void AddVersion(SemVersion version, string uri)
    {
        var dependencyVersion = new DependencyVersion(Name, version, new Uri(uri));
        _versions.Add(dependencyVersion);
    }
    public bool TrySelectVersion(SemVersionRange versionRange, out DependencyVersion? version)
    {
        version = _versions
            .OrderByDescending(x => x.Version)
            .FirstOrDefault(x => versionRange.Contains(x.Version));
        return version != null;
    }
    public DependencyVersion SelectVersion(SemVersionRange versionRange)
    {
        if (!TrySelectVersion(versionRange, out var selectedVersion))
            throw new InvalidOperationException($"None of the package versions for dependency {Name} satisfy version selection criteria {versionRange}");
        return selectedVersion!;

    }
}