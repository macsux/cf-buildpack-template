using System.CommandLine;
using System.CommandLine.Binding;

namespace CloudFoundry.Buildpack.V2.Commands;

public class BuildContextBinder(Argument<string> buildPath, Argument<string> cachePath, Argument<string> depsPath, Argument<int> buildpackIndex, string hookName) : BinderBase<BuildContext>
{
    protected override BuildContext GetBoundValue(BindingContext bindingContext)
    {
        return new BuildContext()
        {
            BuildDirectory = new WellKnownVariablePath(bindingContext.ParseResult.GetValueForArgument(buildPath), WellKnownVariablePath.HomeDirectory),
            CacheDirectory = (VariablePath)bindingContext.ParseResult.GetValueForArgument(cachePath)!,
            DependenciesDirectory = new WellKnownVariablePath(bindingContext.ParseResult.GetValueForArgument(depsPath), WellKnownVariablePath.HomeDirectory / "deps"), // (VariablePath)bindingContext.ParseResult.GetValueForArgument(depsPath)!,
            BuildpackIndex = bindingContext.ParseResult.GetValueForArgument(buildpackIndex),
            IsFinalize = hookName == Lifecycle.Finalize,
        };
    }
}