using Semver;
using Tomlyn;
using Tomlyn.Model;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class DependencyDemand : IDependencyDemand
{
    public DependencyDemand()
    {
        
    }
    public DependencyDemand(string name)
    {
        Name = name;
    }
    public string Name { get; init; } = null!;

    object? IDependencyDemand.Metadata => Metadata;
    public virtual TomlTable? Metadata { get; set; }
    public DependencyDemand<TMetadata> AsDependency<TMetadata>()
        where TMetadata : class, new()
    {
        var dependency = new DependencyDemand<TMetadata>(Name);
        if (Metadata != null)
        {
            dependency.Metadata = Toml.ToModel<TMetadata>(Toml.FromModel(Metadata));
        }

        return dependency;
    }
}

// public static class DependencyDemandExtensions
// {
//     public static TMetadata GetMetadata<TMetadata>(this IDependencyDemand dependencyDemand) where TMetadata : class, new()
//     {
//         TMetadata result;
//         if (dependencyDemand.Metadata is TomlTable toml)
//         {
//             result = Toml.ToModel<TMetadata>(Toml.FromModel(toml), options: new TomlModelOptions() { IgnoreMissingProperties = true });
//         }
//         else if (dependencyDemand.Metadata is TMetadata metadata)
//         {
//             result = metadata;
//         }
//         else
//         {
//             result = new TMetadata();
//         }
//
//         return result;
//     }
// }

[PublicAPI]
public class DependencyDemand<TMetadata> : IDependencyDemand 
    where TMetadata : class, new()
{
    public string Name { get; init; } = null!;
    private DependencyDemand()
    {
    }

    public DependencyDemand(string name) : this(name, new())  //: base(name)
    {
        
    }
    
    public DependencyDemand(string name, TMetadata metadata) // : base(name)
    {
        Name = name;
        Metadata = metadata;
    }

    object? IDependencyDemand.Metadata => Metadata;
    public TMetadata? Metadata { get; set; }

    public DependencyDemand ToUntyped()
    {
        return new DependencyDemand(Name)
        {
            Metadata = Metadata != null ? Toml.ToModel(Toml.FromModel(Metadata)) : null
        };
    }

    public DependencyDemand<TMetadata> WithMetadata(TMetadata metadata)
    {
        return new DependencyDemand<TMetadata>(Name, metadata);
    }
    
    public DependencyDemand<TMetadata> WithMetadata(Action<TMetadata> metadataConfig)
    {
        if (metadataConfig == null) throw new ArgumentNullException(nameof(metadataConfig));
        var metadata = Metadata ?? new();
        metadataConfig(metadata);
        return new DependencyDemand<TMetadata>(Name, metadata);
    }
}
[PublicAPI]
public class VersionDependencyDemand : DependencyDemand<VersionRequirement>
{
    public static VersionDependencyDemand Create(string name, SemVersionRange versionRange)
    {
        return new VersionDependencyDemand(name, new VersionRequirement { VersionRange = versionRange });
    }
    protected VersionDependencyDemand(string name, VersionRequirement metadata) : base(name, metadata)
    {
    }
}
[PublicAPI]
public class VersionRequirement
{
    public SemVersionRange VersionRange { get; set; } = SemVersionRange.AllRelease;
}