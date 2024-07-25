using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CloudFoundry.Buildpack.V2.Analyzers;

[Generator]
public class OverrideGenerator : BuildpackExtensionGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {

        var (finder, buildpackClasses) = GetBuildpackClasses(context);
        
        var buildpackClassesDetails = buildpackClasses
            .Select(x =>
                new
                {
                    Class = x.Syntax,
                    Namespace = x.Model.ContainingNamespace,
                    IsPreStartupOverriden = x.Model.BaseType?.Name == "PluginInjectorBuildpack" || x.Syntax.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(y => y.Identifier.Text == "PreStartup" && y.Modifiers.Any(SyntaxKind.OverrideKeyword))
                })
            .ToList();

        foreach (var buildpackClass in buildpackClassesDetails)
        {

            var sourceBuilder = new StringBuilder($@"
namespace {buildpackClass.Namespace};
partial class {buildpackClass.Class.Identifier.Text}
{{
    protected override bool IsPreStartOverridden => {buildpackClass.IsPreStartupOverriden.ToString().ToLower()};
    protected override string ImplementingClassName => ""{buildpackClass.Class.Identifier.Text}"";
    protected override string BuildpackVersion => ""{finder.AssemblyFileVersion}"";
}}  
");
            context.AddSource(buildpackClass.Class.Identifier.Text + ".gs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}