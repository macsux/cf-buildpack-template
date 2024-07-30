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
        stageContext.Buildpacks.Add(RootDirectory / "artifacts" / "hwc-buildpack.zip");
        stageContext.SkipDetect = true;
        var stageResults = await fixture.Stage(stageContext);
        var droplet = await stageResults.GetDroplet();
        // assert staging results
        AbsolutePath contribFile = droplet.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = stageResults.ToLaunchContext();
        var launchResult = await fixture.Launch(launchContext);
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
            ProcessTasks.StartProcess("dotnet", "publish -r win-x64 --self-contained", 
                workingDirectory: appDir,
                logInvocation: true,
                logOutput: true, logger: (type, s) => _output.WriteLine(s))
                .WaitForExit();
        }
        var stageContext = _fixture.CreateStagingContext(publishDir);
        // stageContext.SkipDetect = true;
        stageContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        var stageResults = await fixture.Stage(stageContext);
        var droplet = await stageResults.GetDroplet();
        // assert staging results
        AbsolutePath contribFile = droplet.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = stageResults.ToLaunchContext();
        var launchResult = await fixture.Launch(launchContext);
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
        stageContext.Buildpacks.Add(RootDirectory / "artifacts" / "hwc-buildpack.zip");
        stageContext.SkipDetect = true;
        var stageResults = await _fixture.Stage(stageContext);

        var droplet = await stageResults.GetDroplet();
        // launch the droplet
        var launchContext = stageResults.ToLaunchContext();
        var launchResult = await _fixture.Launch(launchContext);
        // assert how app behaves in running state
        var result = await launchResult.HttpClient.GetAsync("DateTime.aspx");
        var responseContent = await result.Content.ReadAsStringAsync();
        result.IsSuccessStatusCode.Should().BeTrue(responseContent);
        responseContent.Should().Be("Highjacked!");
    }
#endif
}