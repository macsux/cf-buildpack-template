using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CloudFoundry.Buildpack.V2.Analyzers;

[Generator]
public class OverrideGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
           context.RegisterForSyntaxNotifications(() => new ClassFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var finder = (ClassFinder)context.SyntaxReceiver!;
        
        var buildpackClasses = finder.Classes
            .Select(x => new {Syntex = x, Model = context.Compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)!})
            .Where(x => x.Model.InheritsFrom("BuildpackBase") && !x.Syntex.Modifiers.Any(SyntaxKind.AbstractKeyword))
            .ToList();
        
        var buildpackClassesDetails = buildpackClasses
            .Select(x =>
                new
                {
                    Class = x.Syntex,
                    Namespace = x.Model.ContainingNamespace,
                    IsPreStartupOverriden = x.Syntex.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(y => y.Identifier.Text == "PreStartup" && y.Modifiers.Any(SyntaxKind.OverrideKeyword))
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
}}  
");
            context.AddSource(buildpackClass.Class.Identifier.Text + ".gs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}

public static class Extensions
{
    public static bool InheritsFrom(this INamedTypeSymbol symbol, string type)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (current.Name == type)
                return true;
            current = current.BaseType;
        }
        return false;
    }
}
public class ClassFinder : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Classes { get; }
        = new();
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax controller)
        {
            Classes.Add(controller);
        }
    }
}