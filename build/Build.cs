using System.Runtime.CompilerServices;
using CloudFoundry.Buildpack.V2.Build;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

[assembly: InternalsVisibleTo("MyBuildpackTests")]
[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild, IPublishBuildpack, IReleaseGithub, IBuildNugetCache
{
    [Solution] public Solution Solution { get; set; } = null!;
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
#if (IsSupplyBuildpack)
    public bool IsSupplyBuildpack => true;
#else
    public bool IsSupplyBuildpack => false;
#endif
    
    public string BuildpackProjectName => "MyBuildpack";


    static Build()
    {
        Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "true");
    }

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => ((IPublishBuildpack)x).PublishBuildpack);

    [Parameter("Injection Project")]
    public string? InjectionProject { get; set; }
}
