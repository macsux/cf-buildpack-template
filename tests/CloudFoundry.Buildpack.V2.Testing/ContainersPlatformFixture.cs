using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Volumes;
using Newtonsoft.Json;
using Nuke.Common.Tooling;
using static DotNet.Testcontainers.Configurations.UnixFileModes;


namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public abstract class ContainersPlatformFixture : IAsyncLifetime
{
    public virtual TimeSpan DefaultStagingTimeout { get; set; } = TimeSpan.FromMinutes(3);
    public virtual TimeSpan DefaultLaunchTimeout { get; set; } = TimeSpan.FromSeconds(90);
    internal static UnixFileModes ReadPermissions = UserRead | GroupRead | OtherRead;

    internal static UnixFileModes ReadAndExecutePermissions = ReadPermissions | UserExecute | GroupExecute | OtherExecute;
    internal static UnixFileModes ReadWriteAndExecutePermissions = ReadAndExecutePermissions | UserWrite | GroupWrite | OtherWrite;

    // static AsyncLocal<Stream?> _outputStreamContext = new();
    // public static Stream? OutputStream
    // {
    //     get => _outputStreamContext.Value;
    //     set => _outputStreamContext.Value = value;
    // }
    
    internal ContainersPlatformFixture(IMessageSink messageSink)
    {
        MessageSink = messageSink;
    }

    public IImage RootfsImage { get; protected set; } = null!;

    public abstract CloudFoundryStack Stack { get; }

    protected IMessageSink MessageSink { get; set; }

    protected abstract Dictionary<string, string> GetContainerEnvironmentalVariables(CloudFoundryContainerContext context);


    public StageContext CreateStagingContext(AbsolutePath applicationDirectory)
    {
        var baseDir = TestContext.GetTestCaseDirectory();
        var context = new StageContext
        {
            ApplicationDirectory = applicationDirectory,
            DropletDirectory = baseDir / "droplet",
            Stack = Stack
        };

        return context;
    }
    
    // public LaunchContext CreateLaunchContext(StageResults stageContext) => new(stageContext.ToLaunchContext(), Stack);
    public abstract Task InitializeAsync();
    public abstract Task DisposeAsync();
    
    internal abstract AbsolutePath RemoteHome { get; }
    internal  abstract AbsolutePath RemoteTemp { get; }
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
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(TestContext.TestOutputStream,TestContext.TestOutputStream));
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
    public virtual async Task<LaunchResult> Launch(LaunchContext context, CancellationToken cancellationToken = default)
    {
        TestContext.TestOutputHelper?.WriteLine("Launching app....");
        var stopwatch = Stopwatch.StartNew();
        if (cancellationToken == CancellationToken.None)
        {
            cancellationToken = new CancellationTokenSource(DefaultLaunchTimeout).Token;
        }
        
        var containerBuilder = LaunchingContainerConfigurer(CommonContainerConfigurer(new(), context));

        var freeHostPort = NextFreePort();
        var waitStrategy = context.WaitStrategy(WaitStrategy);
        containerBuilder = containerBuilder
                .WithCommand(LaunchCommand.ToArray())
                .WithVolumeMount(context.DropletVolume, RemoteTemp / "droplet")
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
            TestContext.TestOutputHelper?.WriteLine($"App started after {stopwatch.Elapsed:hh\\:mm\\:ss}");
        }
        finally
        {
            try
            {
                var logs = await container.GetLogsAsync(ct: cancellationToken);
                result.Logs = logs;
                TestContext.TestOutputHelper?.WriteLine(logs.ToString());
            }
            catch (Exception ex)
            {
                TestContext.TestOutputHelper?.WriteLine(ex.ToString());
            }
        }
        return result;
    }

    protected virtual Task<IVolume> CreateDropletVolume(CancellationToken cancellationToken = default)
    {
        var dropletVolume = new VolumeBuilder()
            // .WithCleanUp(false)
            .Build();
        return Task.FromResult(dropletVolume);
    }
    public virtual async Task<StageResults> Stage(StageContext context, CancellationToken cancellationToken = default)
    {
        TestContext.TestOutputHelper?.WriteLine("Staging app....");
        var stopwatch = Stopwatch.StartNew();
        if (cancellationToken == CancellationToken.None)
        {
            cancellationToken = new CancellationTokenSource(DefaultStagingTimeout).Token;
        }
        
        var containerBuilder = StagingContainerConfigurer(CommonContainerConfigurer(new(), context)); 
        if (!context.ApplicationDirectory.Exists())
            throw new InvalidOperationException("Application directory doesn't exist");
        if (context.LifecycleDirectory == null || !context.LifecycleDirectory.Exists())
            throw new InvalidOperationException("Lifecycle directory doesn't exist");
        if (context.DropletDirectory == null)
            throw new InvalidOperationException("Value of context.DropletDirectory must not be null");

        context.DropletDirectory.CreateDirectory();
        // if (!OperatingSystem.IsWindows())
        // {
        //     ProcessTasks.StartProcess("chmod", $"777 {context.DropletDirectory}");
        // }
        var stageCommand = new List<string>(StageCommand);
        stageCommand.AddRange(context.Buildpacks.Select(x => x.NameWithoutExtension));
        if (context.SkipDetect)
        {
            stageCommand.Add("-skipDetect");
        }

        var dropletVolume = await CreateDropletVolume(cancellationToken);


        
        containerBuilder = containerBuilder
                .WithCommand(stageCommand.ToArray())
                // .WithVolumeMount(dropletVolume, (RemoteTemp / "droplet").AsLinuxPath(), AccessMode.ReadWrite)
                .WithVolumeMount(dropletVolume, RemoteTemp / "droplet", AccessMode.ReadWrite)
                // .WithResourceMapping(new DirectoryInfo(context.CacheDirectory), (RemoteTemp / "cache").AsLinuxPath())
                .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (RemoteHome / "app").AsLinuxPath(), ReadWriteAndExecutePermissions)
                // .WithBindMount(context.DropletDirectory,  RemoteTemp / "droplet")
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
            await container.DisposeAsync();
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
        // output?.WriteLine($"Droplet Dir: {context.DropletDirectory}");
        var result = new StageResults(this, dropletVolume, context.DropletDirectory, logs)
        {
            Buildpacks = context.Buildpacks.Select(x => x.NameWithoutExtension).ToList(),
        };
        TestContext.TestOutputHelper?.WriteLine($"Finished staging in {stopwatch.Elapsed:hh\\:mm\\:ss}. Droplet stored on volume {dropletVolume.Name}");
        return result;
        
    }

    internal abstract Task<Droplet> GetDroplet(IVolume dropletVolume, AbsolutePath localPath, CancellationToken cancellationToken = default);

    public async Task<LaunchResult> Push(StageContext context, ITestOutputHelper output)
    {
        var stageResult = await Stage(context);
        var launchContext = stageResult.ToLaunchContext();
        var launchResult = await Launch(launchContext);
        return launchResult;
    }
}