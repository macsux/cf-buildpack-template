using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class WindowsStackFixture : ContainersPlatformFixture
{
    internal const string LaunchScriptName = "launch.ps1";
    internal const string StageScriptName = "stage.ps1";
    public WindowsStackFixture(IMessageSink messageSink) : base(messageSink)
    {
        LaunchCommand =  ["powershell", "-Command", RemoteTemp / LaunchScriptName];
        StageCommand =  ["powershell", "-Command", RemoteTemp / StageScriptName];
    }

    public override CloudFoundryStack Stack => CloudFoundryStack.Windows;

    public override async Task InitializeAsync()
    {
        
        DockerHelper.SwitchContainersPlatform(ContainerPlatform.Windows);
        var dockerContextDirectory = DirectoryHelper.CurrentAssemblyFolder / "_empty";
        var dockerfile = "test-windows2019rootfs.Dockerfile";
        FileSystemTasks.EnsureExistingDirectory(dockerContextDirectory);
        FileSystemTasks.CopyFileToDirectory(DirectoryHelper.CurrentAssemblyFolder / dockerfile, dockerContextDirectory, FileExistsPolicy.OverwriteIfNewer);
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfile(dockerfile)
            .WithDockerfileDirectory(dockerContextDirectory)
            .WithImageBuildPolicy(PullPolicy.Missing)
            .WithName(KnownImages.Windows2016fsTest)
            .Build();
        await image.CreateAsync().ConfigureAwait(false);
        
        RootfsImage = image;
    }

    public override async Task DisposeAsync()
    {
        foreach (var container in _containers)
        {
            await container.DisposeAsync();
        }
    }

    protected override AbsolutePath RemoteHome => (AbsolutePath)@"c:\Users\vcap";
    protected override AbsolutePath RemoteTemp => RemoteHome / "appdata" / "local" / "temp";
    protected override IWaitForContainerOS WaitStrategy => Wait.ForWindowsContainer();
    protected override string ContainerImage => KnownImages.Windows2016fsTest;

    protected override Func<ContainerBuilder, ContainerBuilder> StagingContainerConfigurer => _ => _
        .WithResourceMapping(new FileInfo(DirectoryHelper.CurrentAssemblyFolder / StageScriptName), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions);

    protected override Func<ContainerBuilder, ContainerBuilder> LaunchingContainerConfigurer => _ => _
        .WithResourceMapping(new FileInfo(DirectoryHelper.CurrentAssemblyFolder / LaunchScriptName), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions);
    
    List<IContainer> _containers = new();
    public override async Task<LaunchResult> Launch(LaunchContext context, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
    {
        var result = await base.Launch(context, output, cancellationToken);
        _containers.Add(result.Container);
        return result;
    }
}