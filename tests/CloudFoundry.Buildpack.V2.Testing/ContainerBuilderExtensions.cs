using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using NMica.Utils.IO;
using Xunit.Abstractions;
using static DotNet.Testcontainers.Configurations.UnixFileModes;

namespace CloudFoundry.Buildpack.V2.Testing;


public static class ContainerBuilderExtensions
{
    internal static UnixFileModes ReadPermissions = UserRead | GroupRead | OtherRead;

    internal static UnixFileModes ReadAndExecutePermissions = ReadPermissions | UserExecute | GroupExecute | OtherExecute;
    public static async Task<LaunchResult> Launch(this ContainerBuilder containerBuilder, LaunchContext context, ITestOutputHelper? output = null)
    {
        var currentAssemblyFolder = ((AbsolutePath)Assembly.GetExecutingAssembly().Location).Parent;
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        
        AbsolutePath remoteHome, remoteTemp;
        List<string> command;
        String scriptExtension;
        IWaitForContainerOS waitStrategy;
        if (context.Stack == CloudFoundryStack.Windows)
        {
            remoteHome = (AbsolutePath)@"c:\Users\vcap";
            remoteTemp = remoteHome / "appdata" / "local" / "temp";
            command = ["powershell", "-Command", remoteTemp / "launch.ps1"];
            scriptExtension = "ps1";
            waitStrategy = Wait.ForWindowsContainer();
            // containerBuilder = containerBuilder.WithWaitStrategy(Wait.ForWindowsContainer());
        }
        else
        {
            remoteHome = (AbsolutePath)"/home/vcap";
            remoteTemp = (AbsolutePath)"/tmp";
            command = [ remoteTemp / "launch.sh" ];
            scriptExtension = "sh";
            waitStrategy = Wait.ForUnixContainer();
        }

        waitStrategy = waitStrategy.UntilPortIsAvailable(8080);
        containerBuilder = containerBuilder
                .WithCloudFoundryTestStack(context.Stack)
                .WithCommand(command.ToArray())
                .WithResourceMapping(new DirectoryInfo(context.LifecycleDirectory), (remoteTemp / "lifecycle").AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new FileInfo(currentAssemblyFolder / $"launch.{scriptExtension}"), remoteTemp.AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new FileInfo(context.DropletDirectory / "staging_info.yml"), remoteTemp.AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (remoteHome / "app").AsLinuxPath())
                .WithResourceMapping(new DirectoryInfo(context.ProfileDDirectory), (remoteHome / ".profile.d").AsLinuxPath())
                // .WithResourceMapping(new DirectoryInfo(context.DependenciesDirectory), (remoteHome / "deps").AsLinuxPath())
                .WithBindMount(context.DependenciesDirectory,  remoteHome / "deps")
                .WithEnvironment("DEPS_DIR", (remoteHome / "deps").AsLinuxPath())
                .WithEnvironment("PORT", "8080")
                
                .WithWaitStrategy(waitStrategy)
                .WithPortBinding(8080, 8080)
            ;
        var container = containerBuilder.Build();
        var result = new LaunchResult(container);
        try
        {
            await container.StartAsync(cts.Token).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                var logs = await container.GetLogsAsync();
                result.Logs = logs;
            }
            catch (Exception ex)
            {
                output?.WriteLine(ex.ToString());
                // ignore
            }
        }

        
        return result;
    }
    public static async Task<StageResults> Stage(this ContainerBuilder containerBuilder,  StageContext context)
    {
        if (!Directory.Exists(context.ApplicationDirectory))
            throw new InvalidOperationException("Application directory doesn't exist");
        if (!Directory.Exists(context.LifecycleDirectory))
            throw new InvalidOperationException("Lifecycle directory doesn't exist");
        if (context.DropletDirectory == null)
            throw new InvalidOperationException("Value of context.DropletDirectory must not be null");
        
        FileSystemTasks.EnsureExistingDirectory(context.CacheDirectory);
        FileSystemTasks.EnsureExistingDirectory(context.DropletDirectory);
        
        
        var currentAssemblyFolder = ((AbsolutePath)Assembly.GetExecutingAssembly().Location).Parent;

        AbsolutePath remoteHome;
        AbsolutePath remoteTemp;
        string scriptExtension;
        List<string> command;
        if (context.Stack == CloudFoundryStack.Windows)
        {
            remoteHome = (AbsolutePath)@"c:\Users\vcap";
            remoteTemp = remoteHome / "appdata" / "local" / "temp";
            command = ["powershell", "-Command", remoteTemp / "stage.ps1"];
            scriptExtension = "ps1";
        }
        else
        {
            remoteHome = (AbsolutePath)"/home/vcap";
            remoteTemp = (AbsolutePath)"/tmp";
            command = [ remoteTemp / "stage.sh" ];
            scriptExtension = "sh";
        }
        

        command.AddRange(context.Buildpacks.Select(x => x.NameWithoutExtension));
        if (context.SkipDetect)
        {
            command.Add("-skipDetect");
        }
        containerBuilder = containerBuilder
                .WithCloudFoundryTestStack(context.Stack)
                .WithCommand(command.ToArray())
                .WithResourceMapping(new DirectoryInfo(context.LifecycleDirectory), (remoteTemp / "lifecycle").AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new FileInfo(currentAssemblyFolder / $"stage.{scriptExtension}"), remoteTemp.AsLinuxPath(), ReadAndExecutePermissions)
                .WithResourceMapping(new DirectoryInfo(context.CacheDirectory), (remoteTemp / "cache").AsLinuxPath())
                .WithResourceMapping(new DirectoryInfo(context.ApplicationDirectory), (remoteHome / "app").AsLinuxPath())
                .WithBindMount(context.DropletDirectory,  remoteTemp / "droplet")
            ;
        foreach (var buildpackZip in context.Buildpacks)
        {
            containerBuilder = containerBuilder.WithResourceMapping(new FileInfo(buildpackZip), (remoteTemp / "buildpackdownloads").AsLinuxPath());
        }

        var container = containerBuilder.Build();
        await container.StartAsync().ConfigureAwait(false);
        await container.GetExitCodeAsync();
        var logs = await container.GetLogsAsync();
        var result = new StageResults(context.DropletDirectory, logs)
        {
            Buildpacks = context.Buildpacks.Select(x => x.NameWithoutExtension).ToList(),
        };
        return result;
    }
    internal static ContainerBuilder WithCloudFoundryTestStack(this ContainerBuilder containerBuilder, CloudFoundryStack stack)
    {
        // var image = stack switch
        // {
        //     CloudFoundryStack.Cflinuxfs3 => KnownImages.Cflinuxfs3,
        //     CloudFoundryStack.Cflinuxfs4 => KnownImages.Cflinuxfs4,
        //     CloudFoundryStack.Windows => KnownImages.Windows2016fs,
        //     _ => throw new ArgumentOutOfRangeException(nameof(stack), stack, null)
        // };
        var image = KnownImages.GetTestImageForStack(stack);
        // var image = stack switch
        // {
        //     CloudFoundryStack.Cflinuxfs3 => KnownImages.Cflinuxfs3Test,
        //     CloudFoundryStack.Cflinuxfs4 => KnownImages.Cflinuxfs4Test,
        //     CloudFoundryStack.Windows => KnownImages.Windows2016fsTest,
        //     _ => throw new ArgumentOutOfRangeException(nameof(stack), stack, null)
        // };
        
        return containerBuilder
            .WithImage(image)
            .WithEnvironment("CF_STACK", stack.ToString().ToLowerInvariant());
         
    }
    
    
}

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