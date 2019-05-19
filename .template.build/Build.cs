using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Tasks;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Pack);

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(("NuGet API Configuration Key"))]
    readonly string ApiKey;

    [Parameter("NuGet source API endpoint")] 
    readonly string NuGetSource = "https://api.nuget.org/v3/index.json";
    
    [Parameter("Confirm you want to push to NuGet [Y(es) or N(o)]")] 
    readonly string ShouldPush;

    
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / ".template.artifacts";
    const string NugetPackageId = "CloudFoundry.Buildpack.V2";

    
    AbsolutePath LatestPackage => RootDirectory / ".template.artifacts" / $"{NugetPackageId}.{GitVersion.MajorMinorPatch}.nupkg";

    Target Pack => _ => _
        .Executes(() =>
        {
            NuGetTasks.NuGetPack(s => s
                .SetTargetPath(RootDirectory / "buildpack.nuspec")
                .SetNoDefaultExcludes(true)
                .SetVersion(GitVersion.MajorMinorPatch)
                .SetOutputDirectory(ArtifactsDirectory));
        });

    Target Release => _ => _
        .Description("Publishes the package to NuGet feed")
        .DependsOn(Pack)
        .Requires(() => ApiKey)
        .Requires(() => NuGetSource)
        .Requires(() => ShouldPush)
        .Requires(() => ShouldPush.ToLower() == "y" || ShouldPush.ToLower() == "yes")
        .Executes(() =>
        {
            ControlFlow.Assert(FileExists(LatestPackage), $"{LatestPackage} not found");
            NuGetTasks.NuGetPush(s => s
                .SetSource(NuGetSource)
                .SetTargetPath(LatestPackage)
                .SetApiKey(ApiKey));
            Logger.Block($"Package '{NugetPackageId}' version {GitVersion.MajorMinorPatch} published to {NuGetSource}");
        });
}
