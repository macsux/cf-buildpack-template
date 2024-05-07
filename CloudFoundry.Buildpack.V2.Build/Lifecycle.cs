namespace CloudFoundry.Buildpack.V2.Build;

public static class Lifecycle
{
    public const string Detect = "detect";
    public const string Release = "release";
    public const string Supply = "supply";
    public const string Finalize = "finalize";
    public const string PreStartup = "prestartup";
    public static HashSet<string> AllValues { get; } = [..new[] { Detect, Release, Supply, Finalize, PreStartup }];
}