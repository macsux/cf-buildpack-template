namespace CloudFoundry.Buildpack.V2;

public abstract class SupplyBuildpack : BuildpackBase
{
    public sealed override BuildResult Supply(BuildContext context)
    {
        return DoApply(context);
    }

    public sealed override BuildResult Finalize(BuildContext context)
    {
        return new BuildResult();
    }

    public override void Release(ReleaseContext context)
    {
        // does not get called
    }
}