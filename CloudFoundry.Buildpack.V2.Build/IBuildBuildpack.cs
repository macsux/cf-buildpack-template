namespace CloudFoundry.Buildpack.V2.Build;

public interface IBuildBuildpack : 
    IAssemblyInject,
    IPublishBuildpack,
    IUpdateDependencies,
    IReleaseGithub,
    IMakeOfflineBuildpack
{
    static IBuildBuildpack()
    {
        Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "true");
    }
    
}