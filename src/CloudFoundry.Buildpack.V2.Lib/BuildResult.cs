using JetBrains.Annotations;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class BuildResult
{
    public Dictionary<string, ValueAction> EnvironmentalVariables { get; set; } = new();
    public List<string> ProvidedDependencies { get; set; } = new();
}