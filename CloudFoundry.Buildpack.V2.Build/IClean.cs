using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

namespace CloudFoundry.Buildpack.V2.Build;

public interface IClean : IBuildpackBase
{
    Target Clean => _ => _
        .Description("Cleans up **/bin and **/obj folders")
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
        });
}