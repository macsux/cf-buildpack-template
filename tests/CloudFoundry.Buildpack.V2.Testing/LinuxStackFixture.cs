using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Volumes;

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
    internal override async Task<Droplet> GetDroplet(IVolume dropletVolume, AbsolutePath localPath, CancellationToken cancellationToken = default)
    {
        var container = new ContainerBuilder()
            .WithImage(KnownImages.Cflinuxfs4)
            .WithVolumeMount(dropletVolume, RemoteTemp / "droplet")
            .WithBindMount(localPath,  "/droplet")
            .WithCommand("sh", "-c", "tar -xf /tmp/droplet/droplet.tar -C /droplet && chown -R root:root /droplet")
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(TestContext.TestOutputStream,TestContext.TestOutputStream))
            .Build();

        await container.StartAsync(cancellationToken);
        await container.GetExitCodeAsync(cancellationToken);
        return new Droplet(localPath);
    }
    protected override Dictionary<string, string> GetContainerEnvironmentalVariables(CloudFoundryContainerContext context)
    {
        var envVars = new Dictionary<string, string>(context.EnvironmentalVariables);
        envVars.TryAdd("_","/bin/env");
        envVars.TryAdd("CF_INSTANCE_CERT","/etc/cf-instance-credentials/instance.crt");
        envVars.TryAdd("CF_INSTANCE_INTERNAL_IP","10.255.112.62");
        envVars.TryAdd("CF_INSTANCE_IP","10.0.4.12");
        envVars.TryAdd("CF_INSTANCE_KEY","/etc/cf-instance-credentials/instance.key");
        envVars.TryAdd("CF_INSTANCE_PORTS","""[{"internal":8080,"external_tls_proxy":61026,"internal_tls_proxy":61001},{"internal":8080,"internal_tls_proxy":61443},{"internal":2222,"external_tls_proxy":61027,"internal_tls_proxy":61002}]""");
        envVars.TryAdd("CF_SYSTEM_CERT_PATH","/etc/cf-system-certificates");
        envVars.TryAdd("LANG","en_US.UTF-8");
        envVars.TryAdd("LESSCLOSE","/bin/lesspipe %s %s");
        envVars.TryAdd("LESSOPEN","| /bin/lesspipe %s");
        envVars.TryAdd("LS_COLORS","rs");
        envVars.TryAdd("MEMORY_LIMIT",$"{context.VcapApplication.Limits.Mem}m");
        envVars.TryAdd("PATH","/bin:/usr/bin");
        envVars.TryAdd("PWD","/home/vcap");
        envVars.TryAdd("SHLVL","1");
        envVars.TryAdd("TERM","xterm");
        envVars.TryAdd("USER","vcap");
        envVars.TryAdd("CF_INSTANCE_GUID","4f72d8f5-c51b-4adf-6749-06f6");
        envVars.TryAdd("CF_INSTANCE_INDEX","0");
        envVars.TryAdd("DATABASE_URL","mysql2://523eac29c07a4db58da7d6150bb0ce7c:r1jb85k977b1ew0z@82e190a0-be30-4648-8f05-991cc7b34ffe.mysql.service.internal:3306/service_instance_db?reconnect");
        envVars.TryAdd("INSTANCE_GUID","4f72d8f5-c51b-4adf-6749-06f6");
        envVars.TryAdd("INSTANCE_INDEX","0");
        envVars.TryAdd("PORT","8080");
        envVars.TryAdd("VCAP_APP_HOST","0.0.0.0");
        envVars.TryAdd("VCAP_APP_PORT","8080");

        return envVars;
    }

    protected abstract string TestImageDockerfile { get; }

    protected override async Task<IVolume> CreateDropletVolume(CancellationToken cancellationToken = default)
    {
        var dropletVolume = await base.CreateDropletVolume();
        var changeOwner = new ContainerBuilder()
            .WithImage(KnownImages.Cflinuxfs4)
            .WithVolumeMount(dropletVolume, RemoteTemp / "droplet")
            .WithCommand("/bin/bash", "-c", "chown -R 2000:2000 /tmp/droplet")
            .Build();
        await dropletVolume.CreateAsync(cancellationToken).ConfigureAwait(false);

        await changeOwner.StartAsync(cancellationToken);
        await changeOwner.GetExitCodeAsync(cancellationToken);
        return dropletVolume;
    }

    public override async Task InitializeAsync()
    {
        // TestcontainersSettings.Logger = TestContext.GetLogger("Testcontainers");
        DockerHelper.SwitchContainersPlatform(ContainerPlatform.Linux);
        var dockerContextDirectory = DirectoryHelper.CurrentAssemblyFolder / "_empty";
        dockerContextDirectory.CreateDirectory();
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

    internal  override AbsolutePath RemoteHome => (AbsolutePath)"/home/vcap";
    internal  override AbsolutePath RemoteTemp => (AbsolutePath)"/tmp";
    protected override IWaitForContainerOS WaitStrategy => Wait.ForUnixContainer();

    protected override Func<ContainerBuilder, ContainerBuilder> StagingContainerConfigurer => _ => _
        .WithResourceMapping(new FileInfo(DirectoryHelper.CurrentAssemblyFolder / StageScriptName), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions);

    protected override Func<ContainerBuilder, ContainerBuilder> LaunchingContainerConfigurer => _ => _
        .WithResourceMapping(new FileInfo(DirectoryHelper.CurrentAssemblyFolder / LaunchScriptName), RemoteTemp.AsLinuxPath(), ReadAndExecutePermissions);
    
}