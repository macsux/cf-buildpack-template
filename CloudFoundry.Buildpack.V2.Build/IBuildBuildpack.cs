using Nuke.Components;

namespace CloudFoundry.Buildpack.V2.Build;

public interface IBuildBuildpack : 
    IAssemblyInject,
    IPublishBuildpack,
    IUpdateDependencies,
    IReleaseGithub,
    IMakeOfflineBuildpack,
    ITest,
    IPrintLogo;