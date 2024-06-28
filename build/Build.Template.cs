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
#if (IsFinalBuildpack)
    public string Configuration => "Final";
#elif(IsSupplyBuildpack)
    public string Configuration => "Supply";
#elif(IsHttpModuleBuildpack)
    public string Configuration => "HttpModule";
#elif(IsHostedServiceBuildpack)
    public string Configuration => "HostedService";
#endif
    // [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    // readonly string Config = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(("NuGet API Configuration Key"))]
    readonly string NugetApiKey = null!;

    [Parameter("NuGet source API endpoint")] 
    readonly string NuGetSource = "https://api.nuget.org/v3/index.json";
    
    [Parameter("Confirm you want to push to NuGet [Y(es) or N(o)]")] 
    readonly string ShouldPush = null!;
    
    readonly AbsolutePath TemplateConfigDirectory = RootDirectory / ".template.config";
    const string NugetPackageId = "CloudFoundry.Buildpack.V2";
    NerdbankGitVersioning TemplateVersion { get; set; } = null!;
    NerdbankGitVersioning LifecycleVersion { get; set; } = null!;
    NerdbankGitVersioning BuildVersion { get; set; } = null!;
    NerdbankGitVersioning LibVersion { get; set; } = null!;
    NerdbankGitVersioning AnalyzerVersion { get; set; } = null!;
    NerdbankGitVersioning TestVersion { get; set; } = null!;

    
    AbsolutePath TemplatePackage => ArtifactsDirectory / $"{NugetPackageId}.{TemplateVersion.NuGetPackageVersion}.nupkg";
    AbsolutePath LifecyclePackage => ArtifactsDirectory / $"{NugetPackageId}.Lifecycle.{LifecycleVersion.NuGetPackageVersion}.nupkg";
    AbsolutePath TestPackage => ArtifactsDirectory / $"{NugetPackageId}.Testing.{TestVersion.NuGetPackageVersion}.nupkg";
    AbsolutePath BuildPackage => ArtifactsDirectory / $"{NugetPackageId}.Build.{BuildVersion.NuGetPackageVersion}.nupkg";
    AbsolutePath LibPackage => ArtifactsDirectory / $"{NugetPackageId}.Lib.{LibVersion.NuGetPackageVersion}.nupkg";
    AbsolutePath AnalyzerPackage => ArtifactsDirectory / $"{NugetPackageId}.Analyzers.{AnalyzerVersion.NuGetPackageVersion}.nupkg";

    protected override void OnBuildInitialized()
    {
        TemplateVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(TemplateConfigDirectory)
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain template version");
        
        LifecycleVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(RootDirectory / "lifecycle")
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain lifecycle version");
        
        BuildVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(RootDirectory / "CloudFoundry.Buildpack.V2.Build")
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain template version");
        
        LibVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(RootDirectory / "src" / "CloudFoundry.Buildpack.V2.Lib")
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain buildpack library version");
        
        AnalyzerVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(RootDirectory / "src" / "CloudFoundry.Buildpack.V2.Analyzers")
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain analyzer project version");
        
        TestVersion = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(c => c
            .DisableProcessLogOutput()
            .SetProcessWorkingDirectory(RootDirectory / "tests" / "CloudFoundry.Buildpack.V2.Testing")
            .SetFormat(NerdbankGitVersioningFormat.json)
        ).Result ?? throw new InvalidOperationException("Unable to obtain test project version");
    }

    Target PublishTemplate => _ => _
        .Description("Generate template NuGet package")
        .Executes(() =>
        {
            // if dir is empty it causes nuget ignore to glitch out and create folder in output, throw a dummy file in there
            var markerFile = ArtifactsDirectory / "OK";
            markerFile.TouchFile();
            
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
                .SetVersion(LifecycleVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory));
            
            DotNetTasks.DotNetPack(s => s
                .SetProcessWorkingDirectory(RootDirectory)
                .SetProject(RootDirectory / "CloudFoundry.Buildpack.V2.Build" / "CloudFoundry.Buildpack.V2.Build.csproj")
                .SetVersion(BuildVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory)
            );

            DotNetTasks.DotNetPack(s => s
                .SetProcessWorkingDirectory(RootDirectory)
                .SetProject(RootDirectory / "src" / "CloudFoundry.Buildpack.V2.Lib" / "CloudFoundry.Buildpack.V2.Lib.csproj")
                .SetVersion(LibVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory)
            );

            DotNetTasks.DotNetPack(s => s
                .SetProcessWorkingDirectory(RootDirectory)
                .SetProject(RootDirectory / "src" / "CloudFoundry.Buildpack.V2.Analyzers" / "CloudFoundry.Buildpack.V2.Analyzers.csproj")
                .SetVersion(AnalyzerVersion.NuGetPackageVersion)
                .SetOutputDirectory(ArtifactsDirectory)
            );

            DotNetTasks.DotNetPack(s => s
                .SetProcessWorkingDirectory(RootDirectory)
                .SetProject(RootDirectory / "tests" / "CloudFoundry.Buildpack.V2.Testing" / "CloudFoundry.Buildpack.V2.Testing.csproj")
                .SetVersion(TestVersion.NuGetPackageVersion)
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
            
            Assert.True(File.Exists(TemplatePackage), $"{TemplatePackage} not found");
            var packages = new []{ TemplatePackage, LifecyclePackage, BuildPackage, AnalyzerPackage, LibPackage, TestPackage };
            foreach (var package in packages)
            {
                NuGetTasks.NuGet($"push {package} -Source {NuGetSource} -ApiKey {NugetApiKey} -SkipDuplicate");
            }

            // NuGetTasks.NuGetPush(s => s
            //     .SetSource(NuGetSource)
            //     .SetApiKey(NugetApiKey)
            //     .CombineWith(packages, (o,oo) => o
            //         .SetTargetPath(oo)));
            // Log.Logger.Block($"Package '{NugetPackageId}' version {TemplateVersion.NuGetPackageVersion} published to {NuGetSource}");
        });
}