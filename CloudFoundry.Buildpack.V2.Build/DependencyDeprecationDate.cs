namespace CloudFoundry.Buildpack.V2.Manifest;

public partial class DependencyDeprecationDate
{
    public string Date { get; set; } = null!;

    public string Link { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string VersionLine { get; set; } = null!;
}