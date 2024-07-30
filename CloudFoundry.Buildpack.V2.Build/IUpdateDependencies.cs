using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace CloudFoundry.Buildpack.V2.Build;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[PublicAPI]
public interface IUpdateDependencies : IBuildpackBase
{
    Target UpdatePackages => _ => _
        .Executes(() =>
        {
            DotNetRestore(x => x
                .EnableNoCache()
                .SetProcessWorkingDirectory(RootDirectory / "build"));
            DotNetRestore(x => x
                .EnableNoCache()
                .SetProcessWorkingDirectory(RootDirectory));
        });
}