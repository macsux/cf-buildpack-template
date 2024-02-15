using System.Diagnostics;
using System.Text;
using CloudFoundry.Buildpack.V2.Testing;
using CloudFoundry.Buildpack.V2.Testing.Assertions;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using NMica.Utils;
using NMica.Utils.Collections;
using NMica.Utils.IO;
using Xunit.Abstractions;

namespace MyBuildpack.Tests;

public class BuildpackTests
{
    readonly ITestOutputHelper  _output;
    AbsolutePath RootDirectory { get; }
    public BuildpackTests(ITestOutputHelper  output)
    {
        _output = output;
        RootDirectory = (AbsolutePath)new DirectoryInfo(Directory.GetCurrentDirectory())
            .DescendantsAndSelf(x => x.Parent!)
            .FirstOrDefault(x => x.GetDirectories(".nuke").Any())
            ?.FullName;
    }

    [Fact]
    public async Task RunBuilderLinux()
    {
        DockerHelper.SwitchContainersPlatform(PlatformFamily.Linux, _output);
        var appDir = RootDirectory / "tests" / "fixtures" / "testapp";
        var context = BuildpackTestContext.Generate(appDir, CloudFoundryStack.Cflinuxfs4);
        // context.SkipDetect = true;
        
        context.Buildpacks.Add( RootDirectory / "artifacts" / "MyBuildpack-linux-x64-1.0.1-79bcfd7134.zip");
        context.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        using var stageResults = await new ContainerBuilder()
            .Stage(context);
        _output.WriteLine(stageResults.Logs.ToString());
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
    }
    [Fact]
    public async Task RunLauncherDotnet()
    {
        DockerHelper.SwitchContainersPlatform(PlatformFamily.Linux, _output);
        var dropletDir = (AbsolutePath)@"C:\projects\cf-buildpack-template\tests\MyBuildpack.Tests\bin\Debug\net8.0\RunBuilderDotnet-8dc2d47752ca435\droplet";
        var context = LaunchContext.FromDropletDirectory(dropletDir, CloudFoundryStack.Cflinuxfs4);
        var launchResult = await new ContainerBuilder()
            .Launch(context);
        _output.WriteLine(launchResult.Logs.ToString());

        var result = await launchResult.HttpClient.GetAsync("/");
        result.IsSuccessStatusCode.Should().BeTrue();
    }
    
    [Fact]
    public async Task RunBuilderDotnet()
    {
        DockerHelper.SwitchContainersPlatform(PlatformFamily.Linux, _output);
        var appDir = (AbsolutePath)@"C:\projects\DotnetInjectionExperiment\src\SampleApp";
        var context = BuildpackTestContext.Generate(appDir, CloudFoundryStack.Cflinuxfs4);
        
        context.Buildpacks.Add( RootDirectory / "artifacts" / "dotnet-core_buildpack-cached-cflinuxfs4-v2.4.22+1707253006.zip");
        using var stageResults = await new ContainerBuilder()
            .Stage(context);
        _output.WriteLine(stageResults.Logs.ToString());
        _output.WriteLine($"Results Dir: {stageResults.ApplicationDirectory}");
    }
    
    [Fact]
    public async Task RunBuilderWindows()
    {
        DockerHelper.SwitchContainersPlatform(PlatformFamily.Windows);
        var appDir = RootDirectory / "tests" / "fixtures" / "testapp";
        var context = BuildpackTestContext.Generate(appDir, CloudFoundryStack.Windows);
        context.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        context.Buildpacks.Add( RootDirectory / "artifacts" / "MyBuildpack-win-x64-1.0.1-79bcfd7134.zip");
        context.SkipDetect = true;
        using var stageResults = await new ContainerBuilder()
            .Stage(context);
        
        _output.WriteLine(stageResults.Logs.ToString());

        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
    }
}