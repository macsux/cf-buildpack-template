using CloudFoundry.Buildpack.V2.Build;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Tools.NuGet;
using Serilog;

partial class Build
{

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Config = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(("NuGet API Configuration Key"))]
    readonly string NugetApiKey = null!;

    [Parameter("NuGet source API endpoint")] 
    readonly string NuGetSource = "https://api.nuget.org/v3/index.json";
    
    [Parameter("Confirm you want to push to NuGet [Y(es) or N(o)]")] 
    readonly string ShouldPush = null!;
    
    readonly AbsolutePath TemplateConfigDirectory = RootDirectory / ".template.config";
    const string NugetPackageId = "CloudFoundry.Buildpack.V2";
    NerdbankGitVersioning TemplateVersion { get; set; } = null!;

    
    AbsolutePath LatestPackage => RootDirectory / ".template.artifacts" / $"{NugetPackageId}.{TemplateVersion.NuGetPackageVersion}.nupkg";

    protected override void OnBuildInitialized()
    {
        TemplateVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(TemplateConfigDirectory)
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain template version");
    }

    Target PublishTemplate => _ => _
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
                .SetVersion(TemplateVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory));
            
            NuGetTasks.NuGetPack(s => s
                .SetTargetPath(RootDirectory / "lifecycle" / "CloudFoundry.Buildpack.V2.Lifecycle.nuspec")
                .SetNoDefaultExcludes(true)
                .EnableNoPackageAnalysis()
                .SetVersion(TemplateVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory));

            DotNetTasks.DotNetPack(s => s
                .SetProcessWorkingDirectory(RootDirectory)
                .SetProject("MyBuildpack.Template.sln")
                .SetVersion(TemplateVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory)
            );
            
            markerFile.DeleteFile();
        });

    Target ReleaseTemplate => _ => _
        .Description("Publishes the template package to NuGet feed")
        .DependsOn(PublishTemplate)
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
            Log.Logger.Block($"Package '{NugetPackageId}' version {TemplateVersion.NuGetPackageVersion} published to {NuGetSource}");
        });
}