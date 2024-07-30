using Assert = Xunit.Assert;

namespace CloudFoundry.Buildpack.V2.Testing;

public abstract class BaseTests
{
    protected readonly ITestOutputHelper _output;
    protected readonly ContainersPlatformFixture _fixture;

    protected BaseTests(ITestOutputHelper output, ContainersPlatformFixture fixture)
    {
        _output = output;
        _fixture = fixture;
        // ContainersPlatformFixture.OutputStream = new TestOutputStream(output);
        TestContext.TestOutputHelper = output;
    }
}