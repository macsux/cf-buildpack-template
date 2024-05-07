using Newtonsoft.Json;

namespace CloudFoundry.Buildpack.V2.Testing.Models;

[PublicAPI]
public class VcapServices : Dictionary<string, List<VcapServiceBinding>>
{
    public void Add(string serviceType, VcapServiceBinding binding)
    {
        if(!TryGetValue(serviceType, out var bindings))
        {
            bindings = new List<VcapServiceBinding>();
            Add(serviceType, bindings);
        }
        bindings.Add(binding);
    }
}
[PublicAPI]
public class VcapServiceBinding
{
    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("binding_guid")] public string? BindingGuid { get; set; }

    [JsonProperty("binding_name")] public string? BindingName { get; set; }

    [JsonProperty("instance_guid")] public string? InstanceGuid { get; set; }

    [JsonProperty("instance_name")] public string? InstanceName { get; set; }

    [JsonProperty("label")] public string? Label { get; set; }

    [JsonProperty("tags")] public List<string> Tags { get; set; } = new();

    [JsonProperty("plan")] public string? Plan { get; set; }

    [JsonProperty("credentials")] public object? Credentials { get; set; }

    [JsonProperty("syslog_drain_url")] public string? SyslogDrainUrl { get; set; }

    [JsonProperty("volume_mounts")] public List<object> VolumeMounts { get; set; } = new();
}