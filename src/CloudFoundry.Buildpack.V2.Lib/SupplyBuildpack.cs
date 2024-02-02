namespace CloudFoundry.Buildpack.V2;

public abstract class SupplyBuildpack : BuildpackBase
{
    public sealed override void Supply(BuildContext context)
    {
        DoApply(context);
    }

    public sealed override void Finalize(BuildContext context)
    {
        // doesn't get called
    }

    public override void Release(ReleaseContext context)
    {
        // does not get called
    }

    // supply buildpacks may get this lifecycle event, but since only one buildpack will be selected if detection is used, it must be final
    // therefore supply buildpacks always must reply with false
    public override bool Detect(DetectContext context) => false;  
}