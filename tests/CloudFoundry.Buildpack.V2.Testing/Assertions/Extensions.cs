using FluentAssertions.Streams;
using JetBrains.Annotations;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2.Testing.Assertions;

public static class Extensions
{
    [Pure]
    public static PathAssertions Should(this AbsolutePath actualValue)
    {
        return new PathAssertions(actualValue);
    }
}