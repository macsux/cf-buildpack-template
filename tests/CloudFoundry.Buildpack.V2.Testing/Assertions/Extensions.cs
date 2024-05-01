namespace FluentAssertions;

public static class Extensions
{
    [Pure]
    public static PathAssertions Should(this AbsolutePath actualValue)
    {
        return new PathAssertions(actualValue);
    }
}