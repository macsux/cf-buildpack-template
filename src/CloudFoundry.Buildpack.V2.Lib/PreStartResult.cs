namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class PreStartResult
{
    public Dictionary<string, ValueAction> EnvironmentalVariables { get; set; } = new();
}