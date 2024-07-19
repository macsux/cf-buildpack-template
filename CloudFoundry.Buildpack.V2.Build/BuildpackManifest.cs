namespace CloudFoundry.Buildpack.V2.Manifest;


public partial class BuildpackManifest
{
    public DefaultVersion[] DefaultVersions { get; set; } = null!;

    public Dependency[] Dependencies { get; set; } = null!;

    public DependencyDeprecationDate[] DependencyDeprecationDates { get; set; } = null!;

    public object[] ExcludeFiles { get; set; } = null!;

    public string[] IncludeFiles { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string PrePackage { get; set; } = null!;

    public string Stack { get; set; } = null!;

    public object[] UrlToDependencyMap { get; set; } = null!;
}