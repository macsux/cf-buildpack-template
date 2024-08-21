namespace CloudFoundry.Buildpack.V2.TomlModels;

[PublicAPI]
public class BuildpackPlan
{
    public List<DependencyDemand> Entries { get; set; } = [];
}