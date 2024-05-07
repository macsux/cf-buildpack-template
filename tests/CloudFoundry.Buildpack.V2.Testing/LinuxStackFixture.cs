using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;

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

    public override async Task InitializeAsync()
    {
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
            .WithCommand("sh", "-c", "tar -xf /tmp/droplet/droplet.tar -C /tmp/droplet && chown -R vcap /tmp/droplet")
            .Build();
            
        await container.StartAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            await container.GetExitCodeAsync(cancellationToken);
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