using CloudFoundry.Buildpack.V2.Testing;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static CloudFoundry.Buildpack.V2.Testing.DirectoryHelper;

namespace MyBuildpack.Tests;

[Collection(nameof(ContainerPlatform.Linux))]
public class LinuxTests(ITestOutputHelper output, CfLinuxfs4StackFixture fixture) : BaseTests(output, fixture)
{
#if(IsHostedServiceBuildpack)
    [Fact]
    public async Task PushDotnetInjector()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "dotnetapp";
        var stagingContext = _fixture.CreateStagingContext(appDir);
        stagingContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        // you can download dotnet core buildpack off pivnet https://network.pivotal.io/products/dotnet-core-buildpack/
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "dotnet-core_buildpack-cached-cflinuxfs4-v2.4.22+1707253006.zip");
        stagingContext.SkipDetect = true;
        var stageResult = await _fixture.Stage(stagingContext, _output);

        var dropletDir = stageResult.DropletDirectory;
        var context = _fixture.CreateLaunchContext(dropletDir);
        var launchResult = await _fixture.Launch(context, _output);
        var result2 = await launchResult.HttpClient.GetAsync("/");
        result2.IsSuccessStatusCode.Should().BeTrue();
        var result = await launchResult.HttpClient.GetAsync("/hello");
        result.IsSuccessStatusCode.Should().BeTrue();
        var responseBody = await result.Content.ReadAsStringAsync();
        responseBody.Should().Be("Hello world");
    }
#else


    [Fact]
    public async Task PushStaticFile()
    {
        var appDir = RootDirectory / "tests" / "fixtures" / "staticapp";
        var stagingContext = _fixture.CreateStagingContext(appDir);

        stagingContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        // grab a copy of below buildpack here: https://github.com/cloudfoundry/staticfile-buildpack/releases/download/v1.6.10/staticfile-buildpack-cflinuxfs4-v1.6.10.zip
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "staticfile-buildpack-cflinuxfs4-v1.6.10.zip");
        stagingContext.SkipDetect = true;
        var stageResults = await _fixture.Stage(stagingContext, _output);
        // assert staging results
        AbsolutePath contribFile = stageResults.ApplicationDirectory / "public" / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = _fixture.CreateLaunchContext(stageResults.DropletDirectory);
        var launchResult = await _fixture.Launch(launchContext, _output);
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
        var stagingContext = _fixture.CreateStagingContext(appDir);
        // you can download dotnet core buildpack off pivnet https://network.pivotal.io/products/dotnet-core-buildpack/
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "dotnet-core_buildpack-cached-cflinuxfs4-v2.4.22+1707253006.zip");
        var stageResult = await _fixture.Stage(stagingContext, _output);

        var dropletDir = stageResult.DropletDirectory;
        var context = _fixture.CreateLaunchContext(dropletDir);
        var launchResult = await _fixture.Launch(context, _output);
        var result = await launchResult.HttpClient.GetAsync("/");
        result.IsSuccessStatusCode.Should().BeTrue();
    }
#endif
}