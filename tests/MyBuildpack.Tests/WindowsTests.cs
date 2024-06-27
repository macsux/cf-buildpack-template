using CloudFoundry.Buildpack.V2.Testing;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static CloudFoundry.Buildpack.V2.Testing.DirectoryHelper;

namespace MyBuildpack.Tests;

[Collection(nameof(ContainerPlatform.Windows))]
public class WindowsTests(ITestOutputHelper output, WindowsStackFixture fixture) : BaseTests(output, fixture)
{
#if(IsSupplyBuildpack)
    [Fact]
    public async Task PushHwc()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "staticapp";
        var stageContext = _fixture.CreateStagingContext(appDir);

        stageContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        // grab a copy of hwc buildpack (lightweight IIS) here https://network.pivotal.io/products/hwc-buildpack/ and download it to dir below
        stageContext.Buildpacks.Add(RootDirectory / "artifacts" / "hwc_buildpack-cached-windows-v3.1.34+1702922614.zip");
        stageContext.SkipDetect = true;
        var stageResults = await fixture.Stage(stageContext, _output);
        // assert staging results
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = fixture.CreateLaunchContext(stageResults.DropletDirectory);
        var launchResult = await fixture.Launch(launchContext, _output);
        // assert how app behaves in running state
        var result = await launchResult.HttpClient.GetAsync("contrib.txt");
        var responseContent = await result.Content.ReadAsStringAsync();
        result.IsSuccessStatusCode.Should().BeTrue(responseContent);
        responseContent.Should().Be("test");
    }
#endif
    
#if(IsFinalBuildpack)
    [Fact]
    public async Task RunSelfContained()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "dotnetapp";
        var publishDir = appDir / "bin" / "Release" / "net8.0" / "win-x64" / "publish";
        if (!publishDir.Exists())
        {
            ProcessTasks.StartProcess("dotnet", "publish -r win-x64", 
                workingDirectory: appDir,
                logInvocation: true,
                logOutput: true, logger: (type, s) => _output.WriteLine(s)).WaitForExit();
        }
        var stageContext = _fixture.CreateStagingContext(publishDir);

        stageContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        var stageResults = await fixture.Stage(stageContext, _output);
        // assert staging results
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = fixture.CreateLaunchContext(stageResults.DropletDirectory);
        var launchResult = await fixture.Launch(launchContext, _output);
        // assert how app behaves in running state
        var result = await launchResult.HttpClient.GetAsync("/");
        var responseContent = await result.Content.ReadAsStringAsync();
        result.IsSuccessStatusCode.Should().BeTrue(responseContent);
        responseContent.Should().Be("Hello World!");
    }
#endif
    
#if(IsHttpModuleBuildpack)
    [Fact]
    public async Task HttpModuleInjectionTest()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "staticapp";
        var stageContext = _fixture.CreateStagingContext(appDir);

        stageContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        // grab a copy of hwc buildpack (lightweight IIS) here https://network.pivotal.io/products/hwc-buildpack/ and download it to dir below
        stageContext.Buildpacks.Add(RootDirectory / "artifacts" / "hwc_buildpack-cached-windows-v3.1.34+1702922614.zip");
        stageContext.SkipDetect = true;
        var stageResults = await _fixture.Stage(stageContext, _output);
        
        // launch the droplet
        var launchContext = _fixture.CreateLaunchContext(stageResults.DropletDirectory);
        var launchResult = await _fixture.Launch(launchContext, _output);
        // assert how app behaves in running state
        var result = await launchResult.HttpClient.GetAsync("DateTime.aspx");
        var responseContent = await result.Content.ReadAsStringAsync();
        result.IsSuccessStatusCode.Should().BeTrue(responseContent);
        responseContent.Should().Be("Highjacked!");
    }
#endif
}