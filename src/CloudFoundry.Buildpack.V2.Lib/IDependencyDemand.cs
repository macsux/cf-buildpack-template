namespace CloudFoundry.Buildpack.V2;

public interface IDependencyDemand
{
    public string Name { get; }
    public object? Metadata { get; }
    
}