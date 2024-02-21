using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;
using NMica.Utils;
using NMica.Utils.IO;
using Xunit.Abstractions;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class CfLinuxfs3StackFixture(IMessageSink messageSink) : LinuxStackFixture(messageSink)
{
    public override CloudFoundryStack Stack => CloudFoundryStack.Cflinuxfs3;
    protected override string ContainerImage => KnownImages.Cflinuxfs3Test;
    protected override string TestImageDockerfile => "test-cflinuxfs3.Dockerfile";
}
[PublicAPI]
public class CfLinuxfs4StackFixture(IMessageSink messageSink) : LinuxStackFixture(messageSink)
{
    public override CloudFoundryStack Stack => CloudFoundryStack.Cflinuxfs4;
    protected override string ContainerImage => KnownImages.Cflinuxfs4Test;
    protected override string TestImageDockerfile => "test-cflinuxfs4.Dockerfile";
}
[PublicAPI]
public abstract class LinuxStackFixture : ContainersPlatformFixture
{
    internal const string LaunchScriptName = "launch.sh";
    internal const string StageScriptName = "stage.sh";
    public LinuxStackFixture(IMessageSink messageSink) : base(messageSink)
    {
        LaunchCommand =  [ RemoteTemp / LaunchScriptName ];
        StageCommand =  [ RemoteTemp / StageScriptName ];
    }

    protected abstract string TestImageDockerfile { get; }

    public override async Task InitializeAsync()
    {
        DockerHelper.SwitchContainersPlatform(ContainerPlatform.Linux);
        var dockerContextDirectory = DirectoryHelper.CurrentAssemblyFolder / "_empty";
        FileSystemTasks.EnsureExistingDirectory(dockerContextDirectory);
        FileSystemTasks.CopyFileToDirectory(DirectoryHelper.CurrentAssemblyFolder / TestImageDockerfile, dockerContextDirectory, FileExistsPolicy.OverwriteIfNewer);
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfile(TestImageDockerfile)
            .WithDockerfileDirectory(dockerContextDirectory)
            .WithImageBuildPolicy(PullPolicy.Missing)
            .WithName(ContainerImage)
            .Build();
        await image.CreateAsync().ConfigureAwait(false);
        
        RootfsImage = image;
    }

    public override Task DisposeAsync() => Task.CompletedTask;

    protected override AbsolutePath RemoteHome => (AbsolutePath)"/home/vcap";
    protected override AbsolutePath RemoteTemp => (AbsolutePath)"/tmp";
    protected override IWaitForContainerOS WaitStrategy => Wait.ForUnixContainer();

    protected override Func<ContainerBuilder, ContainerBuilder> StagingContainerConfigurer => _ => _
        .WithResourceMapping(new FileInfo(DirectoryHelper.CurrentAssemblyFolder / StageScriptName), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions);

    protected override Func<ContainerBuilder, ContainerBuilder> LaunchingContainerConfigurer => _ => _
        .WithResourceMapping(new FileInfo(DirectoryHelper.CurrentAssemblyFolder / LaunchScriptName), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions);

    public override async Task<StageResults> Stage(StageContext context, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
    {
        var result = await base.Stage(context, output, cancellationToken);
        // gotta untar it in a new container cuz our normal test image runs under vcap which doesn't have enough permissions to extract with stored linux attributes
        var container = new ContainerBuilder()
            .WithImage(KnownImages.Cflinuxfs4)
            .WithBindMount(context.DropletDirectory,  RemoteTemp / "droplet")
            .WithCommand("tar", "-xf", "/tmp/droplet/droplet.tar", "-C", "/tmp/droplet")
            .Build();
            
        await container.StartAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var logs = await container.GetLogsAsync(ct: cancellationToken);
            if (!string.IsNullOrEmpty(logs.Stderr))
            {
                output?.WriteLine(logs.ToString());
            }
        }
        catch (Exception)
        {
            // ignore
        }
        return result;
    }
}