namespace CloudFoundry.Buildpack.V2.TomlModels;

internal class BuildplansToml : Buildplan
{
    [UsedImplicitly]
    public List<Buildplan> Or { get; set; } = new();
}