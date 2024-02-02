namespace CloudFoundry.Buildpack.V2;

public static class Lifecycle
{
    public const string Detect = "detect";
    public const string Release = "release";
    public const string Supply = "supply";
    public const string Finalize = "finalize";
    public const string PreStartup = "prestartup";
    public static string[] AllValues { get; } = new[] { Detect, Release, Supply, Finalize, PreStartup };
}