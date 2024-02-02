using System.CommandLine;
using System.CommandLine.Binding;

namespace CloudFoundry.Buildpack.V2.Commands;

public class ReleaseContextBinder(Argument<string> buildPath) : BinderBase<ReleaseContext>
{
    protected override ReleaseContext GetBoundValue(BindingContext bindingContext)
    {
        return new ReleaseContext()
        {
            BuildDirectory = (VariablePath)bindingContext.ParseResult.GetValueForArgument(buildPath)!
        };
    }
}