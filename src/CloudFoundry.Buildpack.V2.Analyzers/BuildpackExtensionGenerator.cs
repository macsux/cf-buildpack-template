using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CloudFoundry.Buildpack.V2.Analyzers;

public abstract class BuildpackExtensionGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ClassFinder());
    }

    public abstract void Execute(GeneratorExecutionContext context);

    public (ClassFinder, List<(ClassDeclarationSyntax Syntax, INamedTypeSymbol Model)>) GetBuildpackClasses(GeneratorExecutionContext context)
    {

        var finder = (ClassFinder)context.SyntaxReceiver!;

        var buildpackClasses = finder.Classes
            .Select(x => (Syntax: x, Model: context.Compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)!))
            .Where(x => x.Model.InheritsFrom("BuildpackBase") && !x.Syntax.Modifiers.Any(SyntaxKind.AbstractKeyword))
            .ToList();
        return (finder, buildpackClasses);
    }

  
}