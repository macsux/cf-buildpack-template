namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class DetectResult
{
    internal DetectResult()
    {
    }

    internal int ResultCode { get; init; }
    internal List<Buildplan> Buildplans { get; init; } = [];

    public static DetectResult Fail()
    {
        return new DetectResult
        {
            ResultCode = (int)DetectResultCode.Fail
        };
    }

    public static DetectResult Pass() => Pass(new List<Buildplan>());
    public static DetectResult Pass(params IDependencyDemand[] requiredDependencies) => Pass(requiredDependencies.ToList());
    public static DetectResult Pass(List<IDependencyDemand> requiredDependencies)
    {
        var buildplan = new Buildplan
        {
            Requires = requiredDependencies.ToList()
        };
        return Pass(buildplan);
    }

    public static DetectResult Pass(Buildplan buildplan) => Pass(new List<Buildplan> { buildplan });

    public static DetectResult Pass(IEnumerable<Buildplan> buildplans)
    {
        var result = new DetectResult
        {
            ResultCode = (int)DetectResultCode.Pass,
            Buildplans = buildplans.ToList()
        };
        return result;
    }
}