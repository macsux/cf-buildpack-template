namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public abstract class FinalBuildpack : BuildpackBase
{
    public sealed override BuildResult Supply(BuildContext context)
    {
        return DoApply(context);
    }

    public override BuildResult Finalize(BuildContext context) 
    {
        return new BuildResult();
    }

    public override void Release(ReleaseContext context)
    {
        Console.WriteLine("default_process_types:");
        Console.WriteLine($"  web: {GetStartupCommand(context)}");
    }

    /// <summary>
    /// Determines the startup command for the app
    /// </summary>
    /// <returns>Startup command executed by Cloud Foundry to launch the application</returns>
    public abstract string GetStartupCommand(ReleaseContext context);
        
}