using CloudFoundry.Buildpack.V2.Testing;
using NMica.Utils.IO;
using static CloudFoundry.Buildpack.V2.Testing.DirectoryHelper;

namespace MyBuildpack.Tests;

[Collection(nameof(ContainerPlatform.Linux))]
public class LinuxTests(ITestOutputHelper output, CfLinuxfs4StackFixture fixture)
{

    [Fact]
    public async Task PushStaticFile()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "staticapp";
        var stageContext = fixture.CreateStagingContext(appDir);

        stageContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        // grab a copy of below buildpack here: https://github.com/cloudfoundry/staticfile-buildpack/releases/download/v1.6.10/staticfile-buildpack-cflinuxfs3-v1.6.10.zip
        stageContext.Buildpacks.Add(RootDirectory / "artifacts" / "staticfile-buildpack-cflinuxfs4-v1.6.10.zip");
        stageContext.SkipDetect = true;
        var stageResults = await fixture.Stage(stageContext, output);
        // assert staging results
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "public" / "contrib.txt";
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
    
    [Fact]
    public async Task PushDotnetCoreApp()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "dotnetapp";
        var stagingContext = fixture.CreateStagingContext(appDir);
        // you can download dotnet core buildpack off pivnet https://network.pivotal.io/products/dotnet-core-buildpack/
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "dotnet-core_buildpack-cached-cflinuxfs4-v2.4.22+1707253006.zip");
        var stageResult = await fixture.Stage(stagingContext, output);

        var dropletDir = stageResult.DropletDirectory;
        var context = fixture.CreateLaunchContext(dropletDir);
        var launchResult = await fixture.Launch(context, output);
        var result = await launchResult.HttpClient.GetAsync("/");
        result.IsSuccessStatusCode.Should().BeTrue();
    }

}