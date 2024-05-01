using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using static DotNet.Testcontainers.Configurations.UnixFileModes;


namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public abstract class ContainersPlatformFixture : IAsyncLifetime
{
    internal static UnixFileModes ReadPermissions = UserRead | GroupRead | OtherRead;

    internal static UnixFileModes ReadAndExecutePermissions = ReadPermissions | UserExecute | GroupExecute | OtherExecute;
    internal static UnixFileModes ReadWriteAndExecutePermissions = ReadAndExecutePermissions | UserWrite | GroupWrite | OtherWrite;

    internal ContainersPlatformFixture(IMessageSink messageSink)
    {
        MessageSink = messageSink;
    }

    public IImage RootfsImage { get; protected set; } = null!;

    public abstract CloudFoundryStack Stack { get; }

    protected IMessageSink MessageSink { get; set; }

    public StageContext CreateStagingContext(AbsolutePath applicationDirectory, [CallerMemberName] string callingMethod = "test")
    {
        var baseDir = (AbsolutePath)Directory.GetCurrentDirectory() / $"{callingMethod}-{DateTime.Now.Ticks:x}";
        var context = new StageContext
        {
            RootDirectory = DirectoryHelper.RootDirectory,
            CacheDirectory = baseDir / "cache",
            ApplicationDirectory = applicationDirectory,
            DropletDirectory = baseDir / "droplet",
            LifecycleDirectory = DirectoryHelper.RootDirectory / "lifecycle",
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

    ContainerBuilder CommonContainerConfigurer(ContainerBuilder builder, CloudFoundryContainerContext context) => builder
        .WithImage(ContainerImage)
        .WithResourceMapping(new DirectoryInfo(context.LifecycleDirectory), (RemoteTemp / "lifecycle").AsLinuxPath(), ReadAndExecutePermissions)
        // .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (RemoteHome / "app").AsLinuxPath(), ReadWriteAndExecutePermissions)
        
        .WithEnvironment("CF_STACK", Stack.ToString().ToLowerInvariant());
    
    public virtual async Task<LaunchResult> Launch(LaunchContext context, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken == CancellationToken.None)
        {
            cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)).Token;
        }
        
        var containerBuilder = LaunchingContainerConfigurer(CommonContainerConfigurer(new(), context)); 

        var waitStrategy = WaitStrategy.UntilPortIsAvailable(8080);
        containerBuilder = containerBuilder
                .WithCommand(LaunchCommand.ToArray())
                // .WithResourceMapping(new DirectoryInfo(context.LifecycleDirectory), (RemoteTemp / "lifecycle").AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new FileInfo(context.DropletDirectory / "staging_info.yml"), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions)
                // .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (RemoteHome / "app").AsLinuxPath())
                .WithResourceMapping(new DirectoryInfo(context.ProfileDDirectory), (RemoteHome / ".profile.d").AsLinuxPath())
                .WithBindMount(context.ApplicationDirectory, RemoteHome / "app")
                .WithBindMount(context.DependenciesDirectory,  RemoteHome / "deps")
                .WithEnvironment("DEPS_DIR", (RemoteHome / "deps").AsLinuxPath())
                .WithEnvironment("PORT", "8080")
                .WithEnvironment("HOME", RemoteHome / "app")
                .WithWaitStrategy(waitStrategy)
                .WithPortBinding(8080, 8080)
            
            ;
        var container = containerBuilder.Build();
        var result = new LaunchResult(container);
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

        if (!Directory.Exists(context.ApplicationDirectory))
            throw new InvalidOperationException("Application directory doesn't exist");
        if (!Directory.Exists(context.LifecycleDirectory))
            throw new InvalidOperationException("Lifecycle directory doesn't exist");
        if (context.DropletDirectory == null)
            throw new InvalidOperationException("Value of context.DropletDirectory must not be null");
        
        FileSystemTasks.EnsureExistingDirectory(context.CacheDirectory);
        FileSystemTasks.EnsureExistingDirectory(context.DropletDirectory);
        var stageCommand = new List<string>(StageCommand);
        stageCommand.AddRange(context.Buildpacks.Select(x => x.NameWithoutExtension));
        if (context.SkipDetect)
        {
            stageCommand.Add("-skipDetect");
        }
        containerBuilder = containerBuilder
                // .WithCloudFoundryTestStack(context.Stack)
                .WithCommand(stageCommand.ToArray())
                // .WithResourceMapping(new DirectoryInfo(context.LifecycleDirectory), (RemoteTemp / "lifecycle").AsLinuxPath(), ReadAndExecutePermissions)
                // .WithResourceMapping(new FileInfo(currentAssemblyFolder / $"stage.{scriptExtension}"), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new DirectoryInfo(context.CacheDirectory), (RemoteTemp / "cache").AsLinuxPath())
                .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (RemoteHome / "app").AsLinuxPath(), ReadWriteAndExecutePermissions)
                // .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (RemoteHome / "app").AsLinuxPath())
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
                output?.WriteLine(logs.ToString());
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