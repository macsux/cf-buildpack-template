using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Serilog;
using Project = Nuke.Common.ProjectModel.Project;

namespace CloudFoundry.Buildpack.V2.Build;

public interface IBuildpackBase : INukeBuild
{
    public string BuildpackProjectName { get; }
    Project BuildpackProject => Solution.GetAllProjects(BuildpackProjectName).Single();

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath WorkDirectory => TemporaryDirectory / "pack";

    Solution Solution { get; }

    [Parameter]
    string Configuration => TryGetValue(() => Configuration) ?? "Debug";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    [NerdbankGitVersioning(UpdateBuildNumber = true)] NerdbankGitVersioning GitVersion => TryGetValue(() => GitVersion)!;
    [Parameter("Target CF stack type - 'windows' or 'linux'. Determines buildpack runtime (Framework or Core). Default is both")]
    StackType Stack => TryGetValue<StackType?>(() => Stack) ?? StackType.Windows | StackType.Linux;
    IEnumerable<PublishTarget> PublishCombinations
    {
        get
        {
            if (Stack.HasFlag(StackType.Linux))
                yield return new PublishTarget {Stack = StackType.Linux, Framework = "net8.0", Runtime = "linux-x64"};
            if (Stack.HasFlag(StackType.Windows))
                yield return new PublishTarget {Stack = StackType.Windows, Framework = "net48", Runtime = "win-x64"};
        }
    }

    Target EnsureCleanWorkDirectory => _ => _
        .Unlisted()
        .Executes(() =>
        {
            WorkDirectory.CreateOrCleanDirectory();
            Log.Information("Cleaned out {WorkDirectory}", WorkDirectory);
        });
    
    string GetPackageZipName(string runtime) => $"{BuildpackProjectName}-{runtime}-{GitVersion.SemVer1}.zip";
    
}