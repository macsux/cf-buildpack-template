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
    private List<Dependency> SortForDeclaration(ICollection<Dependency> list)
    {
        var queue = new Queue<Dependency>(list);
        var sorted = new List<Dependency>();
    
        Dependency? first = null;
    
        var processed = new HashSet<(string,string)>();
        while(queue.Count > 0)
        {
            var item = queue.Dequeue();
            item.Composition ??= new List<Dependency>();
            if(item == first && queue.Count > 0)
            {
            
                throw new Exception($"Circular reference detected");
            }
            if(item.Composition.Count > 0 && item.Composition.Select(x => (x.Name, x.Version)).Except(processed).Any())
            {
                if(first == null)
                {
                    first = item;
                }
                queue.Enqueue(item);
                continue;
            }
            sorted.Add(item);
            processed.Add((item.Name, item.Version));
            first = null;
        
        }

        return sorted;
    }

    string ToCamelCase(string input) => $"{input[0].ToString().ToLower()}{input.Substring(1, input.Length - 1)}";
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
            Dictionary<string,string> dependencyProperties = new();

            Dictionary<(string, string), string> versionPackageToLocalVariableMap = new();
            var dependencies = SortForDeclaration(manifest.Dependencies);
            foreach (var namedDependency in dependencies.GroupBy(x => x.Name))
            {
                var dependencyName = namedDependency.Key;
                var parts = Regex.Split(dependencyName, "[-\\._]")
                    .Select(x => x[0].ToString().ToUpper() + x.Substring(1, x.Length - 1))
                    .ToList();

                var pascalCaseName = string.Join("", parts);
                var fieldName = $"_{ToCamelCase(pascalCaseName)}";
                
                initBlock.AppendLine($"        {fieldName} = new DependencyPackage(\"{dependencyName}\");");
                dependencyProperties.Add(pascalCaseName, fieldName);
                foreach (var versionedDependency in namedDependency)
                {
                    var versionVariableName = $"{ToCamelCase(pascalCaseName)}_{versionedDependency.Version.Replace(".","_").Replace("-","_")}";
                    
                    versionedDependency.Composition ??= [];
                    if (versionedDependency.Composition.Count > 0)
                    {
                        var slices = new List<string>();
                        foreach (var part in versionedDependency.Composition)
                        {
                            var partVariable = versionPackageToLocalVariableMap[(part.Name, part.Version)];
                            string include = "";
                            string exclude = "";
                            if(part.Include?.Any() ?? false)
                                include = $"include: [{string.Join(",", part.Include.Select(x => $"\"{x}\""))}]";
                            if(part.Exclude?.Any() ?? false)
                                exclude = $"exclude: [{string.Join(",", part.Exclude.Select(x => $"\"{x}\""))}]";
                            var sliceParameters = string.Join(",", [include, exclude]);
                            slices.Add($"           {partVariable}.Slice({sliceParameters})");
                        }
                        initBlock.AppendLine($"        var {versionVariableName} = {fieldName}.AddVersion(SemVersion.Parse(\"{versionedDependency.Version}\", SemVersionStyles.Any), new DependencyVersion[]");
                        initBlock.AppendLine("        {");
                        initBlock.AppendLine(string.Join(",\n", slices));
                        initBlock.AppendLine("        });");
                    }
                    else
                    {
                        initBlock.AppendLine($"        var {versionVariableName} = {fieldName}.AddVersion(SemVersion.Parse(\"{versionedDependency.Version}\", SemVersionStyles.Any), \"{versionedDependency.Uri}\");");

                    }
                    versionPackageToLocalVariableMap.Add((dependencyName, versionedDependency.Version), versionVariableName);
                    // sourceBuilder.AppendLine($"\tpublic static Dependency {pascalCaseName}_v{versionedDependency.Version.Replace(".", "_")} {{ get; }} = new Dependency(\"{versionedDependency.Name}\", \"{versionedDependency.Version}\");");
                }

                // var latest = namedDependency.OrderByDescending(x => x.Version).First();
                // sourceBuilder.AppendLine($"\tpublic static Dependency {pascalCaseName} {{ get; }} = new Dependency(\"{namedDependency.Key}\", \"{latest.Version}\");");
            }

            initBlock.AppendLine($"        AllDependencies = new DependencyPackage[]{{ {string.Join(",", dependencyProperties.Values)} }};");
            var propsDeclarationBlock = string.Join("\n", dependencyProperties.Select(kv => $$"""
                                                                                              private static DependencyPackage {{kv.Value}} = null!;
                                                                                              public static DependencyPackage {{kv.Key}} { get { Init(); return {{kv.Value}}; } }
                                                                                              """));
            var classSrc = $$"""
                             using Semver;
                             namespace {{buildpackClass.Model.ContainingNamespace}};
                             public static partial class {{buildpackName}}Dependencies
                             {
                                private static bool _isInit = false;
                                 private static void Init()
                                 {
                                    if(_isInit) return;
                             {{initBlock}}
                                    _isInit = true;
                                 }
                             
                                 public static IEnumerable<DependencyPackage> AllDependencies { get; private set; }
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