using Nuke.Common;
using Nuke.Common.Utilities;
using Serilog;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

public static class SerilogExtensions
{
    public static IDisposable Block(this ILogger logger, string text)
    {
        return DelegateDisposable.CreateBracket(
            () =>
            {
                var formattedBlockText = text
                    .Split(new[] { EnvironmentInfo.NewLine }, StringSplitOptions.None);

                logger.Information("");
                logger.Information("╬" + '═'.Repeat(text.Length + 5));
                foreach (var line in formattedBlockText)
                {
                    logger.Information("║ {Text}", line);
                }
                logger.Information("╬" + '═'.Repeat(Math.Max(text.Length - 4, 2)));
                logger.Information("");
            });
    }
    
}