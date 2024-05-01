using System.IO;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;

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
    readonly string Config = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(("NuGet API Configuration Key"))]
    readonly string NugetApiKey;

    [Parameter("NuGet source API endpoint")] 
    readonly string NuGetSource = "https://api.nuget.org/v3/index.json";
    
    [Parameter("Confirm you want to push to NuGet [Y(es) or N(o)]")] 
    readonly string ShouldPush;

    
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / ".template.artifacts";
    const string NugetPackageId = "CloudFoundry.Buildpack.V2";

    
    AbsolutePath LatestPackage => RootDirectory / ".template.artifacts" / $"{NugetPackageId}.{GitVersion.MajorMinorPatch}.nupkg";

    Target Pack => _ => _
        .Description("Generate template NuGet package")
        .Executes(() =>
        {
            // if dir is empty it causes nuget ignore to glitch out and create folder in output, throw a dummy file in there
            
            var markerFile = ArtifactsDirectory / "OK";
            markerFile.TouchFile();
            // File.WriteAllText(markerFile, string.Empty);
            
            NuGetTasks.NuGetPack(s => s
                .SetTargetPath(RootDirectory / "buildpack.nuspec")
                .SetNoDefaultExcludes(true)
                .EnableNoPackageAnalysis()
                .SetVersion(GitVersion.MajorMinorPatch)
                .SetOutputDirectory(ArtifactsDirectory));
            markerFile.DeleteFile();
        });

    Target Release => _ => _
        .Description("Publishes the package to NuGet feed")
        .DependsOn(Pack)
        .Requires(() => NugetApiKey)
        .Requires(() => NuGetSource)
        .Requires(() => ShouldPush)
        .Requires(() => ShouldPush.ToLower() == "y" || ShouldPush.ToLower() == "yes")
        .Executes(() =>
        {
            
            Assert.True(File.Exists(LatestPackage), $"{LatestPackage} not found");
            NuGetTasks.NuGetPush(s => s
                .SetSource(NuGetSource)
                .SetTargetPath(LatestPackage)
                .SetApiKey(NugetApiKey));
            Log.Logger.Block($"Package '{NugetPackageId}' version {GitVersion.MajorMinorPatch} published to {NuGetSource}");
        });
}
