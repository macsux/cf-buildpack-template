using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CloudFoundry.Buildpack.V2.Manifest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CloudFoundry.Buildpack.V2.Analyzers;

[Generator]
public class DependencyGenerator : BuildpackExtensionGenerator
{

    public override void Execute(GeneratorExecutionContext context)
    {
        try
        {


            var (finder, buildpackClasses) = GetBuildpackClasses(context);
            var buildpackClass = buildpackClasses.Single();
            var buildpackName = buildpackClass.Syntax.Identifier.ToString();
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            // var buildpackSourceBuilder = new StringBuilder();
            // var knownPackagesBuilder = new StringBuilder();
            // var manifestYaml = context.AdditionalFiles.FirstOrDefault(x => x.Path.EndsWith("manifest.yml"))?.GetText()?.ToString();
            var manifestYaml = context.AdditionalFiles.FirstOrDefault()?.GetText()?.ToString();
            if (manifestYaml == null)
                return;
            // sourceBuilder.AppendLine("/*");
            var manifest = deserializer.Deserialize<BuildpackManifest?>(manifestYaml) ?? new();
            // sourceBuilder.AppendLine(context.AdditionalFiles.FirstOrDefault()?.Path);
            manifest.Dependencies ??= Array.Empty<Dependency>();
            // buildpackSourceBuilder.AppendLine();
            var initBlock = new StringBuilder();
            List<string> dependencyProperties = new();
            foreach (var namedDependency in manifest.Dependencies.GroupBy(x => x.Name))
            {
                var dependencyName = namedDependency.Key;
                var parts = Regex.Split(dependencyName, "[-\\._]")
                    .Select(x => x[0].ToString().ToUpper() + x.Substring(1, x.Length - 1))
                    .ToList();

                var pascalCaseName = string.Join("", parts);

                initBlock.AppendLine($"        {pascalCaseName} = new DependencyPackage(\"{dependencyName}\");");
                dependencyProperties.Add(pascalCaseName);
                foreach (var versionedDependency in namedDependency)
                {
                    // sourceBuilder.AppendLine($"\tpublic static Dependency {pascalCaseName}_v{versionedDependency.Version.Replace(".", "_")} {{ get; }} = new Dependency(\"{versionedDependency.Name}\", \"{versionedDependency.Version}\");");
                    initBlock.AppendLine($"        {pascalCaseName}.AddVersion(SemVersion.Parse(\"{versionedDependency.Version}\", SemVersionStyles.Any), \"{versionedDependency.Uri}\");");
                }

                // var latest = namedDependency.OrderByDescending(x => x.Version).First();
                // sourceBuilder.AppendLine($"\tpublic static Dependency {pascalCaseName} {{ get; }} = new Dependency(\"{namedDependency.Key}\", \"{latest.Version}\");");
            }

            initBlock.AppendLine($"        AllDependencies = new DependencyPackage[]{{ {string.Join(",", dependencyProperties)} }};");
            var propsDeclarationBlock = string.Join("\n", dependencyProperties.Select(name => $"    public static DependencyPackage {name} {{ get; }}"));
            var classSrc = $$"""
                             using Semver;
                             namespace {{buildpackClass.Model.ContainingNamespace}};
                             public static partial class {{buildpackName}}Dependencies
                             {
                                 static {{buildpackName}}Dependencies()
                                 {
                             {{initBlock}}
                                 }
                             
                                 public static IEnumerable<DependencyPackage> AllDependencies { get; }
                             {{propsDeclarationBlock}}

                             }
                             """;
            // sourceBuilder.AppendLine("*/");
            // if(!File.Exists())
            context.AddSource("manifest.g.cs", SourceText.From(classSrc, Encoding.UTF8));
        }
        catch (Exception e)
        {
            throw new Exception(e.StackTrace.ToString());
        }
    }
}