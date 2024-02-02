using System.CommandLine;
using System.CommandLine.Binding;

namespace CloudFoundry.Buildpack.V2.Commands;

public class BuildContextBinder(Argument<string> buildPath, Argument<string> cachePath, Argument<string> depsPath, Argument<int> buildpackIndex) : BinderBase<BuildContext>
{
    protected override BuildContext GetBoundValue(BindingContext bindingContext)
    {
        return new BuildContext()
        {
            BuildDirectory = (VariablePath)bindingContext.ParseResult.GetValueForArgument(buildPath)!,
            CacheDirectory = (VariablePath)bindingContext.ParseResult.GetValueForArgument(cachePath)!,
            DependenciesDirectory = (VariablePath)bindingContext.ParseResult.GetValueForArgument(depsPath)!,
            BuildpackIndex = bindingContext.ParseResult.GetValueForArgument(buildpackIndex),
        };
    }
}