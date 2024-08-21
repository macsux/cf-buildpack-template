namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public static class BuildResultExtensions
{
    public static void Set(this Dictionary<string, ValueAction> envVars, string key, string value)
    {
        envVars[key] = new SetValueAction(key, value);
    }
    public static void Append(this Dictionary<string, ValueAction> envVars, string key, string value, string delimiter = ";")
    {
        if (envVars.ContainsKey(key))
        {
            throw new InvalidOperationException($"Cannot append - environmental variables already has a SetValueAction for key {key}");
        }

        envVars[key] = new AppendValueAction(key, value, delimiter);
    }
}