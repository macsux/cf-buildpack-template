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
        (appDir / "bin").DeleteDirectory();
        (appDir / "obj").DeleteDirectory();
        var stagingContext = _fixture.CreateStagingContext(appDir);
        stagingContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "dotnet-core-buildpack.zip");
        stagingContext.SkipDetect = true;
        var stageResult = await _fixture.Stage(stagingContext);

        var droplet = await stageResult.GetDroplet();
        var context = stageResult.ToLaunchContext();
        var launchResult = await _fixture.Launch(context);
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
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "staticfile-buildpack.zip");
        stagingContext.SkipDetect = true;
        var stageResults = await _fixture.Stage(stagingContext);
        // assert staging results
        using var droplet = await stageResults.GetDroplet();
        AbsolutePath contribFile = droplet.ApplicationDirectory / "public" / "contrib.txt";
        contribFile.Should().BeExistingFile();
        File.ReadAllText(contribFile).Should().Be("test");
        
        // launch the droplet
        var launchContext = stageResults.ToLaunchContext();
        var launchResult = await _fixture.Launch(launchContext);
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
        // use your own buildpack below - dotnet-core-buildpack is provided as an example of working final buildpack for test structure purposes
        // stagingContext.Buildpacks.Add( RootDirectory / "artifacts" / "latest" / "linux-x64" / "buildpack.zip");
        stagingContext.Buildpacks.Add(RootDirectory / "artifacts" / "dotnet-core-buildpack.zip");
        var stageResult = await _fixture.Stage(stagingContext);

        var context = stageResult.ToLaunchContext();
        var launchResult = await _fixture.Launch(context);
        var result = await launchResult.HttpClient.GetAsync("/");
        result.IsSuccessStatusCode.Should().BeTrue();
    }
#endif
}