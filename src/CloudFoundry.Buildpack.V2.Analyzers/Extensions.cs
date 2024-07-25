using Microsoft.CodeAnalysis;

namespace CloudFoundry.Buildpack.V2.Analyzers;

public static class Extensions
{
    public static bool InheritsFrom(this INamedTypeSymbol symbol, string type)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (current.Name == type)
                return true;
            current = current.BaseType;
        }
        return false;
    }
}