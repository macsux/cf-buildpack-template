using CloudFoundry.Buildpack.V2.MyBuildpack;
using CloudFoundry.Buildpack.V2.Testing;
using FluentAssertions;
using NMica.Utils.IO;
using Semver;

namespace CloudFoundry.Buildpack.V2.Lib.Tests;

public class LibraryTests
{
    readonly ITestOutputHelper _output;

    public LibraryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // [Fact]
    // public void EnsureDependencies()
    // {
    //     MyBuildpackDependencies.DotnetSdk.Name.Should().Be("dotnet-sdk");
    //     MyBuildpackDependencies.DotnetSdk.Versions.Should().NotBeEmpty();
    //     MyBuildpackDependencies.DotnetSdk.Versions.First().Parts.Should().BeEmpty();
    //     
    //     MyBuildpackDependencies.DotnetRuntime.Name.Should().Be("dotnet-runtime");
    //     var runtimeVersion = MyBuildpackDependencies.DotnetRuntime.SelectVersion(SemVersionRange.Inclusive(new SemVersion(8), new SemVersion(9)));
    //     runtimeVersion.Name.Should().Be("dotnet-runtime");
    //     runtimeVersion.Version.ToString().Should().Be("8.0.7");
    //     runtimeVersion.Parts.Should().NotBeEmpty();
    //     var slice = runtimeVersion.Parts.First();
    //     slice.Name.Should().Be(MyBuildpackDependencies.DotnetSdk.Name);
    //     slice.Version.ToString().Should().Be("8.0.107");
    //     // slice.Include.Should().BeEquivalentTo(["/host/**", "/shared/Microsoft.NETCore.App/{partof-version}", "/dotnet"]);
    //     slice.Folder = (VariablePath)@"C:\projects\cf-buildpack-template\artifacts\sdk";
    //     foreach (var (absolutePath, relativePath) in runtimeVersion.SelectFiles())
    //     {
    //         _output.WriteLine(absolutePath);
    //     }
    // }


    [Fact]
    public void DependencyTests()
    {
        var depsFolder = (VariablePath)TestContext.GetTestCaseDirectory().ToString();
        var part1 = new DependencyVersion("part1", new SemVersion(1, 0, 0), new Uri("http://part1"), include: new[]{"/part1/**"}, exclude: new[]{"/part1/b*"}, dependenciesDirectory: depsFolder);
        var part2 = new DependencyVersion("part2", new SemVersion(2, 0, 0), new Uri("http://part2"), include: new[]{"/part2/{version}/**", "/composite/{partof-version}/**"},  dependenciesDirectory: depsFolder);
        var part1Files = new[]
        {
            part1.Folder!.CurrentAbsolutePath / "part1" / "a.txt",
            part1.Folder!.CurrentAbsolutePath / "part1" / "b.txt",
            part1.Folder!.CurrentAbsolutePath / "part1" / "c.txt",
        };
        var part2Files = new[]
        {
            part2.Folder!.CurrentAbsolutePath / "composite" / "3.0.0" / "1.txt",
            part2.Folder.CurrentAbsolutePath / "part2" / "2.0.0" / "2.txt",
            part2.Folder.CurrentAbsolutePath / "part2" / "x.txt"
        };
        foreach (var file in part1Files.Union(part2Files))
        {
            FileSystemTasks.Touch(file);
        }
        var compositeDependency = new DependencyVersion("composite", new SemVersion(3,0,0), parts: [part1, part2]);
        var files = compositeDependency.SelectFiles().Select(x => x.RelativePath.ToUnixRelativePath().ToString());
        files.Should().BeEquivalentTo(new[]
        {
            "part1/a.txt",
            "part1/c.txt",
            "part2/2.0.0/2.txt",
            "composite/3.0.0/1.txt"
        });
    }
}