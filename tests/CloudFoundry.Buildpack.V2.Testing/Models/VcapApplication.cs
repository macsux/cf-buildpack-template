using Newtonsoft.Json;

namespace CloudFoundry.Buildpack.V2.Testing.Models;

[PublicAPI]
public class VcapApplication
{
    [JsonProperty("application_id")]
    public string? ApplicationId { get; set; }

    [JsonProperty("application_name")]
    public string? ApplicationName { get; set; }

    [JsonProperty("application_uris")] public List<string> ApplicationUris { get; set; } = new();

    [JsonProperty("application_version")]
    public string? ApplicationVersion { get; set; }

    [JsonProperty("cf_api")]
    public string? CfApi { get; set; }

    [JsonProperty("limits")] public Limits Limits { get; set; } = new();

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonProperty("organization_name")]
    public string? OrganizationName { get; set; }

    [JsonProperty("space_id")]
    public string? SpaceId { get; set; }

    [JsonProperty("space_name")]
    public string? SpaceName { get; set; }

    [JsonProperty("uris")] public List<string> Uris { get; set; } = new();

    [JsonProperty("users")]
    public object? Users { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
}
[PublicAPI]
public class Limits
{
    [JsonProperty("disk")]
    public int? Disk { get; set; }

    [JsonProperty("fds")]
    public int? Fds { get; set; }

    [JsonProperty("mem")]
    public int? Mem { get; set; }
}

