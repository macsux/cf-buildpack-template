using System.Diagnostics;
using System.Text;
using CloudFoundry.Buildpack.V2.Testing;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using NMica.Utils;
using NMica.Utils.Collections;
using NMica.Utils.IO;
using Xunit.Abstractions;
using static CloudFoundry.Buildpack.V2.Testing.DirectoryHelper;

namespace MyBuildpack.Tests;

[Collection(nameof(ContainerPlatform.Linux))]
public class BuildpackTests(ITestOutputHelper output, CfLinuxfs4StackFixture fixture)
{

    [Fact]
    public async Task RunDotnetCoreDroplet()
    {
        var dropletDir = (AbsolutePath)@"C:\projects\cf-buildpack-template\tests\MyBuildpack.Tests\bin\Debug\net8.0\RunBuilderDotnet-8dc2d47752ca435\droplet";
        var context = LaunchContext.FromDropletDirectory(dropletDir, CloudFoundryStack.Cflinuxfs4);
        var launchResult = await fixture.Launch(context, output);
        var result = await launchResult.HttpClient.GetAsync("/");
        result.IsSuccessStatusCode.Should().BeTrue();
    }
    
    [Fact]
    public async Task StageDotnetCoreApp()
    {
        var appDir = (AbsolutePath)@"C:\projects\DotnetInjectionExperiment\src\SampleApp";
        var context = fixture.CreateStagingContext(appDir);
        context.Buildpacks.Add(RootDirectory / "artifacts" / "dotnet-core_buildpack-cached-cflinuxfs4-v2.4.22+1707253006.zip");
        var stageResult = fixture.Stage(context, output);
    }

    [Fact]
    public async Task StageSampleBuildpack()
    {
        // DockerHelper.SwitchContainersPlatform(ContainerPlatform.Linux);
        var appDir = RootDirectory / "tests" / "fixtures" / "testapp";
        // var context = StageContext.Generate(appDir, CloudFoundryStack.Cflinuxfs4);
        // context.SkipDetect = true;
        var context = fixture.CreateStagingContext(appDir);
        
        context.Buildpacks.Add( RootDirectory / "artifacts" / "MyBuildpack-linux-x64-1.0.1-79bcfd7134.zip");
        context.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        // using var stageResults = await new ContainerBuilder()
        //     .Stage(context);
        var stageResults = await fixture.Stage(context, output);
        // _output.WriteLine(stageResults.Logs.ToString());
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
    }
}