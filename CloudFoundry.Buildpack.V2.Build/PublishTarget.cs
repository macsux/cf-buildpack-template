namespace CloudFoundry.Buildpack.V2.Build;

public struct PublishTarget
{
    public string Framework { get; set; }
    public string Runtime { get; set; }
}