using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CloudFoundry.Buildpack.V2.Analyzers;

public class ClassFinder : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Classes { get; }
        = new();
    public string? AssemblyFileVersion { get; set; }
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax controller)
        {
            Classes.Add(controller);
        }
        
        if (syntaxNode is AttributeSyntax attr && attr.Name.ToString().Contains("AssemblyFileVersion"))
        {
            AssemblyFileVersion = attr.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()?.Trim('"');
        }
    }
}