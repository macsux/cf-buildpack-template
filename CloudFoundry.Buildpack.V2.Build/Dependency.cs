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
}