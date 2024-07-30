using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

internal class WebConfig : IDisposable
{
    readonly XmlDocument _doc;
    readonly XmlNamespaceManager _ns;

    const string MsNamespace = "urn:schemas-microsoft-com:asm.v1";

    readonly string _appDir;
    readonly string _webConfigPath;

    public WebConfig(string path)
    {
        _webConfigPath = path;
        _appDir = Path.GetDirectoryName(path) ?? throw new ArgumentException($"Unable to obtain directory path for web.config path '{path}'", path);
        _doc = new();
        var nav = _doc.CreateNavigator();
        _ns	= new XmlNamespaceManager(nav!.NameTable);
        _ns.AddNamespace("ms", MsNamespace);
        _doc.Load(path);
    }

    public XmlElement Configuration => _doc.DocumentElement!;
    public XmlElement Configuration_SystemWebServer => GetOrCreateElement(Configuration, "system.webServer");
    public XmlElement Configuration_SystemWebServer_Modules => GetOrCreateElement(Configuration_SystemWebServer, "modules");
    public XmlElement Configuration_Runtime => GetOrCreateElement(Configuration, "runtime");
    public XmlElement Configuration_Runtime_AssemblyBinding => GetOrCreateElement(Configuration_Runtime, "assemblyBinding", MsNamespace);

    public void AddHttpModule(string assemblyQualifiedName)
    {
        var typeName = assemblyQualifiedName.Split(',').First().Split('.').Last();
        var httpModuleNode = _doc.CreateElement("add");
        httpModuleNode.SetAttribute("name", typeName);
        httpModuleNode.SetAttribute("type", assemblyQualifiedName);
        Configuration_SystemWebServer_Modules.AppendChild(httpModuleNode);
    }

    XmlElement GetOrCreateElement(XmlElement parent, string elementName, string? nameSpace = null)
    {
        var node = parent[elementName];
        if (node == null)
        {
            node = _doc.CreateElement(elementName, nameSpace);
            parent.AppendChild(node);
        }

        return node;
    }
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Assembly load is only used to read assembly name which should be trim safe")]

    public void CreateAssemblyBindings(AbsolutePath assembliesDir)
    {
        var libraryFiles = Directory.EnumerateFiles(assembliesDir, "*.dll", SearchOption.AllDirectories).ToList();

        // var probingDirs = libraryFiles
        //     .Select(x => ((AbsolutePath)x).Parent)
        //     .Select(x => PathConstruction.GetRelativePath(_appDir, x))
        //     .ToList();
		
        // Console.WriteLine("Applying assembly loading information to web.config for following:");
        var assemblyNames = libraryFiles.Select(file =>
            {
                AssemblyName assemblyName = null!;
                try
                {
                    assemblyName = Assembly.LoadFile(file).GetName();
                }
                catch (Exception)
                {
                    Console.Error.WriteLine($"{file} is not a valid .NET assembly");
                }

                return (File: file, AssemblyName: assemblyName);
            })
            .Where(x => x.AssemblyName != null)
            .ToList();
        
        foreach (var assembliesByName in assemblyNames.GroupBy(x => x.AssemblyName.Name))
        {
            var assemblyIdentity = assembliesByName.First().AssemblyName;
            var assemblyPublicKeyToken = string.Join(string.Empty, Array.ConvertAll(assemblyIdentity.GetPublicKeyToken(), x => x.ToString("X2"))).ToLower();
            var publicKeyTokenSelector = !string.IsNullOrEmpty(assemblyPublicKeyToken) ? $" and @publicKeyToken='{assemblyPublicKeyToken}'" : string.Empty;
            var cultureSelector = !string.IsNullOrEmpty(assemblyIdentity.CultureName) ? $" and culture='{assemblyIdentity.CultureName}'" : string.Empty;
            var dependentAssemblyNode = (XmlElement?)Configuration_Runtime_AssemblyBinding.SelectSingleNode($"ms:dependentAssembly[ms:assemblyIdentity/@name='{assemblyIdentity.Name}'{publicKeyTokenSelector}{cultureSelector}]", _ns);
            if (dependentAssemblyNode == null)
            {
                dependentAssemblyNode = _doc.CreateElement("dependentAssembly", MsNamespace);
                Configuration_Runtime_AssemblyBinding.AppendChild(dependentAssemblyNode);
                var assemblyIdentityNode = _doc.CreateElement("assemblyIdentity", MsNamespace);
                dependentAssemblyNode.AppendChild(assemblyIdentityNode);
                // var assemblyIdentityNode = _doc.CreateElement("assemblyIdentity", MsNamespace);
                if (!string.IsNullOrEmpty(assemblyPublicKeyToken))
                {
                    assemblyIdentityNode.SetAttribute("publicKeyToken", string.Join(string.Empty, Array.ConvertAll(assemblyIdentity.GetPublicKeyToken(), x => x.ToString("X2"))).ToLower());
                }

                if (!string.IsNullOrEmpty(assemblyIdentity.CultureName))
                {
                    assemblyIdentityNode.SetAttribute("culture", assemblyIdentity.CultureName);
                }

                assemblyIdentityNode.SetAttribute("name", assemblyIdentity.Name);
                // dependentAssemblyNode.AppendChild(assemblyIdentityNode);

            }

            foreach (var assemblyVersion in assembliesByName)
            {

                
                var codeBaseNode = (XmlElement?)dependentAssemblyNode.SelectSingleNode($"ms:codeBase[@version='{assemblyVersion.AssemblyName.Version}']", _ns);
                if (codeBaseNode == null)
                {
                    codeBaseNode = _doc.CreateElement("codeBase", MsNamespace);
                    codeBaseNode.SetAttribute("version", assemblyVersion.AssemblyName.Version.ToString());
                    codeBaseNode.SetAttribute("href", PathConstruction.GetRelativePath(_appDir, assemblyVersion.File));
                    dependentAssemblyNode.AppendChild(codeBaseNode);
                }
            }
        }
    }

    public void SaveAs(string path) => _doc.Save(path);

    public void Dispose()
    {
        _doc.Save(_webConfigPath);
    }
}