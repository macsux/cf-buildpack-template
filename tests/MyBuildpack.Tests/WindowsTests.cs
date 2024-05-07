using CloudFoundry.Buildpack.V2.Testing;
using Nuke.Common.IO;
using static CloudFoundry.Buildpack.V2.Testing.DirectoryHelper;

namespace MyBuildpack.Tests;

[Collection(nameof(ContainerPlatform.Windows))]
public class WindowsTests(ITestOutputHelper output, WindowsStackFixture fixture)
{
   
    [Fact]
    public async Task PushHwc()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "staticapp";
        var stageContext = fixture.CreateStagingContext(appDir);

        stageContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "win-x64" / "buildpack.zip");
        // grab a copy of hwc buildpack (lightweight IIS) here https://network.pivotal.io/products/hwc-buildpack/ and download it to dir below
        stageContext.Buildpacks.Add(RootDirectory / "artifacts" / "hwc_buildpack-cached-windows-v3.1.34+1702922614.zip");
        stageContext.SkipDetect = true;
        var stageResults = await fixture.Stage(stageContext, output);
        // assert staging results
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = fixture.CreateLaunchContext(stageResults.DropletDirectory);
        var launchResult = await fixture.Launch(launchContext, output);
        // assert how app behaves in running state
        var result = await launchResult.HttpClient.GetAsync("contrib.txt");
        var responseContent = await result.Content.ReadAsStringAsync();
        result.IsSuccessStatusCode.Should().BeTrue(responseContent);
        responseContent.Should().Be("test");
    }

}