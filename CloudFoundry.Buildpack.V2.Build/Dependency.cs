using System.Diagnostics.CodeAnalysis;

namespace CloudFoundry.Buildpack.V2.Manifest;

public partial class Dependency
{
    public string Name { get; set; } = null!;

    public string? Sha256 { get; set; }

    public string Source { get; set; } = null!;

    public string SourceSha256 { get; set; } = null!;

    public string? Uri { get; set; }

    public string Version { get; set; } = null!;
    public string File { get; set; } = null!;
    
    public List<Dependency>? Composition { get; set; }
    public List<string>? Include { get; set; }
    public List<string>? Exclude { get; set; }
}