using CloudFoundry.Buildpack.V2.Testing;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using NMica.Utils.IO;
using Xunit.Abstractions;
using static CloudFoundry.Buildpack.V2.Testing.DirectoryHelper;

namespace MyBuildpack.Tests;

[Collection(nameof(ContainerPlatform.Windows))]
public class WindowsTests(ITestOutputHelper output, WindowsStackFixture fixture)
{
    [Fact]
    public async Task StageHwc()
    {
        var appDir = (AbsolutePath)@"C:\projects\funnyquotes\artifacts\FunnyQuotesUIForms";
        var context = fixture.CreateStagingContext(appDir);
        context.Buildpacks.Add(RootDirectory / "artifacts" / "hwc_buildpack-cached-windows-v3.1.34+1702922614.zip");
        context.SkipDetect = true;
        var stageResults = await fixture.Stage(context, output);
        output.WriteLine($"Droplet Dir: {stageResults.DropletDirectory}");
    }

    [Fact]
    public async Task RunHwc()
    {
        var dropletDir = (AbsolutePath)@"C:\projects\cf-buildpack-template\tests\MyBuildpack.Tests\bin\Debug\net8.0\StageHwc-8dc3140fb6222e2\droplet";
        var context = fixture.CreateLaunchContext(dropletDir);
        var launchResult = await fixture.Launch(context, output);
        
        var result = await launchResult.HttpClient.GetAsync("/");
        var responseContent = await result.Content.ReadAsStringAsync();
        result.IsSuccessStatusCode.Should().BeTrue(responseContent);
    }

    [Fact]
    public async Task StageCurrentBuildpackWindows()
    {
        // DockerHelper.SwitchContainersPlatform(ContainerPlatform.Windows);
        var appDir = RootDirectory / "tests" / "fixtures" / "testapp";
        var context = fixture.CreateStagingContext(appDir);
        context.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        context.Buildpacks.Add( RootDirectory / "artifacts" / "MyBuildpack-win-x64-1.0.1-79bcfd7134.zip");
        context.SkipDetect = true;
        // using var stageResults = await new ContainerBuilder()
        //     .Stage(context);
        var stageResults = await fixture.Stage(context);
        
        output.WriteLine(stageResults.Logs.ToString());

        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
    }
}