namespace CloudFoundry.Buildpack.V2;

public class DependencyMetadata
{
    public bool Launch { get; set; } = true;
    public bool Build { get; set; }
}