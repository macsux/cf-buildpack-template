using DotNet.Testcontainers.Containers;

namespace CloudFoundry.Buildpack.V2.Testing;

public class LaunchResult
{
    public LaunchResult(IContainer container)
    {
        Container = container;
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        
    }
    public (string StdOut, string ErrOut) Logs { get; internal set; }

    public HttpClient HttpClient { get; }
    public IContainer Container { get; }
    
}