namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class Buildplan
{
    public List<IDependencyDemand> Provides { get; init; } = new();
    public List<IDependencyDemand> Requires { get; init;  } = new();
    
}