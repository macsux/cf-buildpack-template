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

    protected override Dictionary<string, string> GetContainerEnvironmentalVariables(CloudFoundryContainerContext context)
    {
        var envVars = new Dictionary<string, string>(context.EnvironmentalVariables);
        envVars.TryAdd("ALLUSERSPROFILE", @"C:\ProgramData");
        envVars.TryAdd("APPDATA", @"C:\Users\vcap\AppData\Roaming");
        envVars.TryAdd("CF_INSTANCE_ADDR", "10.0.4.14:40008");
        envVars.TryAdd("CF_INSTANCE_CERT", "/etc/cf-instance-credentials/instance.crt");
        envVars.TryAdd("CF_INSTANCE_INTERNAL_IP", "172.30.2.39");
        envVars.TryAdd("CF_INSTANCE_IP", "10.0.4.14");
        envVars.TryAdd("CF_INSTANCE_KEY", "/etc/cf-instance-credentials/instance.key");
        envVars.TryAdd("CF_INSTANCE_PORT", "40008");
        envVars.TryAdd("CF_INSTANCE_PORTS",
            """[{"external":40008,"internal":8080,"external_tls_proxy":40010,"internal_tls_proxy":61001},{"external":40008,"internal":8080,"internal_tls_proxy":61443},{"external":40009,"internal":2222,"external_tls_proxy":40011,"internal_tls_proxy":61002}]""");
        envVars.TryAdd("CF_SYSTEM_CERT_PATH", "/etc/cf-system-certificates");
        envVars.TryAdd("CommonProgramFiles", @"C:\Program Files\Common Files");
        envVars.TryAdd("CommonProgramFiles(x86)", @"C:\Program Files (x86)\Common Files");
        envVars.TryAdd("CommonProgramW6432", @"C:\Program Files\Common Files");
        envVars.TryAdd("COMPUTERNAME", "32838D5B-66E9-4");
        envVars.TryAdd("ComSpec", @"C:\Windows\system32\cmd.exe");
        envVars.TryAdd("DriverData", @"C:\Windows\System32\Drivers\DriverData");
        envVars.TryAdd("LANG", "en_US.UTF-8");
        envVars.TryAdd("LOCALAPPDATA", @"C:\Users\vcap\AppData\Local");
        envVars.TryAdd("MEMORY_LIMIT",$"{context.VcapApplication.Limits.Mem}m");
        envVars.TryAdd("NUMBER_OF_PROCESSORS", "2");
        envVars.TryAdd("OS", "Windows_NT");
        envVars.TryAdd("PATH", @"C:\Windows\system32;C:\Windows;C:\Windows\System32\Wbem;C:\Windows\System32\WindowsPowerShell\v1.0");
        envVars.TryAdd("PATHEXT", ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC");
        envVars.TryAdd("PROCESSOR_ARCHITECTURE", "AMD64");
        envVars.TryAdd("PROCESSOR_IDENTIFIER", "Intel64 Family 6 Model 79 Stepping 0, GenuineIntel");
        envVars.TryAdd("PROCESSOR_LEVEL", "6");
        envVars.TryAdd("PROCESSOR_REVISION", "4f00");
        envVars.TryAdd("ProgramData", @"C:\ProgramData");
        envVars.TryAdd("ProgramFiles", @"C:\Program Files");
        envVars.TryAdd("ProgramFiles(x86)", @"C:\Program Files (x86)");
        envVars.TryAdd("ProgramW6432", @"C:\Program Files");
        envVars.TryAdd("PROMPT", "$P$G");
        envVars.TryAdd("PSModulePath", @"%ProgramFiles%\WindowsPowerShell\Modules;C:\Windows\system32\WindowsPowerShell\v1.0\Modules");
        envVars.TryAdd("PUBLIC", @"C:\Users\Public");
        envVars.TryAdd("SystemDrive", "C:");
        envVars.TryAdd("SystemRoot", @"C:\Windows");
        envVars.TryAdd("TEMP", @"C:\Users\vcap\AppData\Local\Temp");
        envVars.TryAdd("TERM", "xterm");
        envVars.TryAdd("TMP", @"C:\Users\vcap\AppData\Local\Temp");
        envVars.TryAdd("USER", "");
        envVars.TryAdd("USERDOMAIN", "32838D5B-66E9-4");
        envVars.TryAdd("USERNAME", "vcap");
        envVars.TryAdd("USERPROFILE", @"C:\Users\vcap");
        envVars.TryAdd("VCAP_PLATFORM_OPTIONS", """{"credhub-uri":"https://credhub.service.cf.internal:8844"}""");
        envVars.TryAdd("windir", @"C:\Windows");
        envVars.TryAdd("CF_INSTANCE_GUID","4f72d8f5-c51b-4adf-6749-06f6");
        envVars.TryAdd("CF_INSTANCE_INDEX","0");
        envVars.TryAdd("INSTANCE_GUID","4f72d8f5-c51b-4adf-6749-06f6");
        envVars.TryAdd("INSTANCE_INDEX","0");
        envVars.TryAdd("PORT","8080");
        envVars.TryAdd("VCAP_APP_HOST","0.0.0.0");
        envVars.TryAdd("VCAP_APP_PORT","8080");

        return envVars;
    }

    public override async Task InitializeAsync()
    {
        
        DockerHelper.SwitchContainersPlatform(ContainerPlatform.Windows);
        var dockerContextDirectory = DirectoryHelper.CurrentAssemblyFolder / "_empty";
        var dockerfile = "test-windows2019rootfs.Dockerfile";
        dockerContextDirectory.CreateDirectory();
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