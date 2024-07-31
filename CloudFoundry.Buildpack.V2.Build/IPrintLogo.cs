using FIGlet;
using FIGlet.Utility;
using JetBrains.Annotations;
using Nuke.Common;

namespace CloudFoundry.Buildpack.V2.Build;

[PublicAPI]
public interface IPrintLogo : IBuildpackBase
{
    public string? Logo => BuildpackProjectName;
    public void PrintLogo()
    {
        if(Logo == null)
            return;
        var font = FIGfont.FromEmbeddedResource(@"FigletFonts.ANSIShadow.flf", typeof(IPrintLogo));
        var figDriver = new FIGdriver { Font = font };

        figDriver.Write(Logo);
        Console.WriteLine(figDriver.ToString());
    }
}