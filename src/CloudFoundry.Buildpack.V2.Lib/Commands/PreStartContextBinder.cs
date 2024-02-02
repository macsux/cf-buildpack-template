using System.CommandLine;
using System.CommandLine.Binding;

namespace CloudFoundry.Buildpack.V2.Commands;

public class PreStartContextBinder(Argument<int> index) : BinderBase<PreStartupContext>
{
    protected override PreStartupContext GetBoundValue(BindingContext bindingContext)
    {
        return new PreStartupContext()
        {
            BuildpackIndex = bindingContext.ParseResult.GetValueForArgument(index),
            AppDirectory = VariablePath.FromEnvironmentalVariable("HOME"),
            DependenciesDirectory = VariablePath.FromEnvironmentalVariable("DEPS_DIR")
        };
    }
}