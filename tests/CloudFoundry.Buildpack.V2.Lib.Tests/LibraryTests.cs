using FluentAssertions;
using NMica.Utils.IO;
using Semver;

namespace CloudFoundry.Buildpack.V2.Lib.Tests;

public class LibraryTests
{
    [Fact]
    public void DependencyTests()
    {
        var depsFolder = (VariablePath)Directory.CreateTempSubdirectory().FullName;
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
        var compositeDependency = new DependencyVersion("composite", new SemVersion(3,0,0), new[]{part1, part2});
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