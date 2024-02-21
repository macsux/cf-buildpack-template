namespace CloudFoundry.Buildpack.V2.Testing;

public static class KnownImages
{
    public const string Cflinuxfs3 = "cloudfoundry/cflinuxfs3";
    public const string Cflinuxfs4 = "cloudfoundry/cflinuxfs4";
    public const string Windows2016fs = "cloudfoundry/windows2016fs";
    public const string Windows2016fsTest = "windows2016fs-test";
    public const string Cflinuxfs3Test = "cflinuxfs3-test";
    public const string Cflinuxfs4Test = "cflinuxfs4-test";

    public static string GetTestImageForStack(CloudFoundryStack stack) => stack switch
    {
        CloudFoundryStack.Cflinuxfs3 => Cflinuxfs3Test,
        CloudFoundryStack.Cflinuxfs4 => Cflinuxfs4Test,
        CloudFoundryStack.Windows => Windows2016fsTest,
        _ => throw new ArgumentOutOfRangeException(nameof(stack), stack, null)
    };
}