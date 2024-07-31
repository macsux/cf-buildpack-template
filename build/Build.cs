using System.Runtime.CompilerServices;
using CloudFoundry.Buildpack.V2.Build;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.CloudFoundry;
using Nuke.Components;

[assembly: InternalsVisibleTo("MyBuildpackTests")]

[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild, IBuildBuildpack
{
    IEnumerable<Project> ITest.TestProjects => Partition.GetCurrent(Solution.GetAllProjects("*.Tests"));
    [Solution] public Solution Solution { get; set; } = null!;
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

#if(IsHttpModuleBuildpack)
    public StackType Stack => StackType.Windows;
#endif
    public string BuildpackProjectName => "MyBuildpack";
    
    static Build()
    {
        Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "true");
        Environment.SetEnvironmentVariable("NoLogo", "true");
    }

    public static int Main() => Execute<Build>(x => ((IPublishBuildpack)x).PublishBuildpack);

}
