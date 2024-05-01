namespace CloudFoundry.Buildpack.V2;

public abstract class FinalBuildpack : BuildpackBase
{
    public sealed override void Supply(BuildContext context)
    {
        // do nothing, we always apply in finalize
    }

    public sealed override void Finalize(BuildContext context)
    {
        DoApply(context);
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