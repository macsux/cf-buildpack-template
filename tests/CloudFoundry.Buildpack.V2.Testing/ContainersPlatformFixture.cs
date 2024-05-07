using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using Newtonsoft.Json;
using Nuke.Common.Tooling;
using static DotNet.Testcontainers.Configurations.UnixFileModes;


namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public abstract class ContainersPlatformFixture : IAsyncLifetime
{
    internal static UnixFileModes ReadPermissions = UserRead | GroupRead | OtherRead;

    internal static UnixFileModes ReadAndExecutePermissions = ReadPermissions | UserExecute | GroupExecute | OtherExecute;
    internal static UnixFileModes ReadWriteAndExecutePermissions = ReadAndExecutePermissions | UserWrite | GroupWrite | OtherWrite;

    public Stream? OutputStream { get; set; }
    
    internal ContainersPlatformFixture(IMessageSink messageSink)
    {
        MessageSink = messageSink;
    }

    public IImage RootfsImage { get; protected set; } = null!;

    public abstract CloudFoundryStack Stack { get; }

    protected IMessageSink MessageSink { get; set; }

    protected abstract Dictionary<string, string> GetContainerEnvironmentalVariables(CloudFoundryContainerContext context);


    public StageContext CreateStagingContext(AbsolutePath applicationDirectory, [CallerMemberName] string callingMethod = "test")
    {
        var baseDir = (AbsolutePath)Directory.GetCurrentDirectory() / $"{callingMethod}-{DateTime.Now.Ticks:x}";
        var context = new StageContext
        {
            CacheDirectory = baseDir / "cache",
            ApplicationDirectory = applicationDirectory,
            DropletDirectory = baseDir / "droplet",
            Stack = Stack
        };

        return context;
    }
    
    public LaunchContext CreateLaunchContext(AbsolutePath dropletDirectory) => new(dropletDirectory, Stack);
    public abstract Task InitializeAsync();
    public abstract Task DisposeAsync();
    
    protected abstract AbsolutePath RemoteHome { get; }
    protected abstract AbsolutePath RemoteTemp { get; }
    protected List<string> LaunchCommand { get; init; } = new();
    protected List<string> StageCommand { get; init; } = new();
    protected abstract IWaitForContainerOS WaitStrategy { get; }
    protected abstract string ContainerImage { get; }
    protected virtual Func<ContainerBuilder, ContainerBuilder> StagingContainerConfigurer => builder => builder;
    protected virtual Func<ContainerBuilder, ContainerBuilder> LaunchingContainerConfigurer => builder => builder;

    ContainerBuilder CommonContainerConfigurer(ContainerBuilder builder, CloudFoundryContainerContext context)
    {
        var envVars = GetContainerEnvironmentalVariables(context);
        envVars.TryAdd("VCAP_APPLICATION", JsonConvert.SerializeObject(context.VcapApplication, Formatting.Indented));
        envVars.TryAdd("VCAP_SERVICES", JsonConvert.SerializeObject(context.VcapServices, Formatting.Indented));
        foreach (var (key, value) in envVars)
        {
            builder = builder.WithEnvironment(key, value);
        }
        return builder
            .WithImage(ContainerImage)
            .WithResourceMapping(new DirectoryInfo(context.LifecycleDirectory), (RemoteTemp / "lifecycle").AsLinuxPath(), ReadAndExecutePermissions)
            .WithEnvironment("CF_STACK", Stack.ToString().ToLowerInvariant())
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(OutputStream,OutputStream));
    }
    bool IsFree(int port)
    {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] listeners = properties.GetActiveTcpListeners();
        int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
        return openPorts.All(openPort => openPort != port);
    }

    int NextFreePort(int port = 0)
    {
        port = (port > 0) ? port : new Random().Next(4000, 40000);
        while (!IsFree(port))
        {
            port += 1;
        }
        return port;
    }
    public virtual async Task<LaunchResult> Launch(LaunchContext context, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken == CancellationToken.None)
        {
            cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)).Token;
        }
        
        var containerBuilder = LaunchingContainerConfigurer(CommonContainerConfigurer(new(), context));

        var freeHostPort = NextFreePort();
        var waitStrategy = WaitStrategy.UntilPortIsAvailable(8080);
        containerBuilder = containerBuilder
                .WithCommand(LaunchCommand.ToArray())
                .WithResourceMapping(new FileInfo(context.DropletDirectory / "staging_info.yml"), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new DirectoryInfo(context.ProfileDDirectory), (RemoteHome / ".profile.d").AsLinuxPath())
                .WithBindMount(context.ApplicationDirectory, RemoteHome / "app")
                .WithBindMount(context.DependenciesDirectory,  RemoteHome / "deps")
                .WithEnvironment("DEPS_DIR", (RemoteHome / "deps").AsLinuxPath())
                .WithEnvironment("HOME", RemoteHome / "app")
                .WithWaitStrategy(waitStrategy)
                .WithPortBinding(freeHostPort, 8080)
            ;
        
        var container = containerBuilder.Build();
        var result = new LaunchResult(container, freeHostPort);
        try
        {
            await container.StartAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                var logs = await container.GetLogsAsync(ct: cancellationToken);
                result.Logs = logs;
                output?.WriteLine(logs.ToString());
            }
            catch (Exception ex)
            {
                output?.WriteLine(ex.ToString());
            }
        }
        return result;
    }
    
    public virtual async Task<StageResults> Stage(StageContext context, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken == CancellationToken.None)
        {
            cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)).Token;
        }
        
        var containerBuilder = StagingContainerConfigurer(CommonContainerConfigurer(new(), context)); 
        if (!context.ApplicationDirectory.Exists())
            throw new InvalidOperationException("Application directory doesn't exist");
        if (context.LifecycleDirectory == null || !context.LifecycleDirectory.Exists())
            throw new InvalidOperationException("Lifecycle directory doesn't exist");
        if (context.DropletDirectory == null)
            throw new InvalidOperationException("Value of context.DropletDirectory must not be null");

        context.CacheDirectory.CreateDirectory();
        context.DropletDirectory.CreateDirectory();
        var stageCommand = new List<string>(StageCommand);
        stageCommand.AddRange(context.Buildpacks.Select(x => x.NameWithoutExtension));
        if (context.SkipDetect)
        {
            stageCommand.Add("-skipDetect");
        }
        containerBuilder = containerBuilder
                .WithCommand(stageCommand.ToArray())
                .WithResourceMapping(new DirectoryInfo(context.CacheDirectory), (RemoteTemp / "cache").AsLinuxPath())
                .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (RemoteHome / "app").AsLinuxPath(), ReadWriteAndExecutePermissions)
                .WithBindMount(context.DropletDirectory,  RemoteTemp / "droplet")
            ;
        foreach (var buildpackZip in context.Buildpacks)
        {
            containerBuilder = containerBuilder.WithResourceMapping(new FileInfo(buildpackZip), (RemoteTemp / "buildpackdownloads").AsLinuxPath());
        }

        var container = containerBuilder.Build();
        (string StdOut, string StdErr) logs = default;
        try
        {
            await container.StartAsync(ct: cancellationToken).ConfigureAwait(false);
            await container.GetExitCodeAsync(ct: cancellationToken);
        }
        finally
        {
            try
            {
                logs = await container.GetLogsAsync(ct: cancellationToken);
            }
            catch (Exception)
            {
                // ignore
            }
            
        }
        output?.WriteLine($"Droplet Dir: {context.DropletDirectory}");
        var result = new StageResults(context.DropletDirectory, logs)
        {
            Buildpacks = context.Buildpacks.Select(x => x.NameWithoutExtension).ToList(),
        };
        return result;
        
    }

    public async Task<LaunchResult> Push(StageContext context, ITestOutputHelper output)
    {
        var stageResult = await Stage(context);
        var launchContext = stageResult.ToLaunchContext();
        var launchResult = await Launch(launchContext);
        return launchResult;
    }
}