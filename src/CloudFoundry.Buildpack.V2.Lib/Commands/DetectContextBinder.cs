using System.CommandLine;
using System.CommandLine.Binding;

namespace CloudFoundry.Buildpack.V2.Commands;

public class DetectContextBinder(Argument<string> buildPath) : BinderBase<DetectContext>
{
    protected override DetectContext GetBoundValue(BindingContext bindingContext)
    {
        return new DetectContext()
        {
            BuildDirectory = (VariablePath)bindingContext.ParseResult.GetValueForArgument(buildPath)!
        };
    }
}