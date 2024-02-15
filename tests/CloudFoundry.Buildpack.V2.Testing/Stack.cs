using JetBrains.Annotations;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public enum CloudFoundryStack
{
    Cflinuxfs3,
    Cflinuxfs4,
    Windows
}