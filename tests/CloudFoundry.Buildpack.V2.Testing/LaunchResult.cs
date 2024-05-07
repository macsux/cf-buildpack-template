using DotNet.Testcontainers.Containers;

namespace CloudFoundry.Buildpack.V2.Testing;

public class LaunchResult
{
    public LaunchResult(IContainer container, int hostPort)
    {
        Container = container;
        HostPort = hostPort;
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{HostPort}")
        };
        
    }
    public (string StdOut, string ErrOut) Logs { get; internal set; }

    public HttpClient HttpClient { get; }
    public IContainer Container { get; }
    public int HostPort { get; }
}