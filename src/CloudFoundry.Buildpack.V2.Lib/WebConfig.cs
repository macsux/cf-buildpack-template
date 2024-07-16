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

    public void CreateAssemblyBindings(VariablePath assembliesDir)
    {
        var libraryFiles = Directory.EnumerateFiles(assembliesDir, "*.dll", SearchOption.AllDirectories).ToList();

        // var probingDirs = libraryFiles
        //     .Select(x => ((AbsolutePath)x).Parent)
        //     .Select(x => PathConstruction.GetRelativePath(_appDir, x))
        //     .ToList();
		
        // Console.WriteLine("Applying assembly loading information to web.config for following:");
        foreach (var file in libraryFiles)
        {
            AssemblyName assemblyName;
            try
            {
                assemblyName = Assembly.LoadFile(file).GetName();
            }
            catch (Exception)
            {
                Console.Error.WriteLine($"{file} is not a valid .NET assembly");
                continue;
            }
            // <probing privatePath="bin;bin2\subbin;bin3"/>
            // var probingElement = GetOrCreateElement(Configuration_Runtime_AssemblyBinding, "probing", MsNamespace);
            // var currentProbingDirs = new HashSet<string>(probingElement.GetAttribute("privatePath").Split(';').Where(x => !string.IsNullOrEmpty(x)));
            // if (currentProbingDirs.Count == 0)
            // {
                // currentProbingDirs.Add("bin");
            // }

            // foreach (var probingDir in probingDirs)
            // {
                // currentProbingDirs.Add(probingDir);
            // }
            // probingElement.SetAttribute("privatePath", string.Join(";", currentProbingDirs));
			
            // Console.WriteLine($"- {file}");
            var assemblyPublicKeyToken = string.Join(string.Empty, Array.ConvertAll(assemblyName.GetPublicKeyToken(), x => x.ToString("X2"))).ToLower();
            var publicKeyTokenSelector = !string.IsNullOrEmpty(assemblyPublicKeyToken) ? $" and @publicKeyToken='{assemblyPublicKeyToken}'" : string.Empty;
            var cultureSelector = !string.IsNullOrEmpty(assemblyName.CultureName) ? $" and culture='{assemblyName.CultureName}'" : string.Empty;
			
            var dependentAssemblyNode = (XmlElement?)Configuration_Runtime_AssemblyBinding.SelectSingleNode($"ms:dependentAssembly[ms:assemblyIdentity/@name='{assemblyName.Name}'{publicKeyTokenSelector}{cultureSelector}]", _ns);
            if (dependentAssemblyNode == null)
            {
                dependentAssemblyNode = GetOrCreateElement(Configuration_Runtime_AssemblyBinding, "dependentAssembly", MsNamespace);
                var assemblyIdentityNode = GetOrCreateElement(dependentAssemblyNode, "assemblyIdentity", MsNamespace);
                // var assemblyIdentityNode = _doc.CreateElement("assemblyIdentity", MsNamespace);
                if (!string.IsNullOrEmpty(assemblyPublicKeyToken))
                {
                    assemblyIdentityNode.SetAttribute("publicKeyToken", string.Join(string.Empty, Array.ConvertAll(assemblyName.GetPublicKeyToken(), x => x.ToString("X2"))).ToLower());
                }

                if (!string.IsNullOrEmpty(assemblyName.CultureName))
                {
                    assemblyIdentityNode.SetAttribute("culture", assemblyName.CultureName);
                }

                assemblyIdentityNode.SetAttribute("name", assemblyName.Name);
                // dependentAssemblyNode.AppendChild(assemblyIdentityNode);

            }

            var codeBaseNode = (XmlElement?)dependentAssemblyNode.SelectSingleNode($"ms:codeBase[@version='{assemblyName.Version}']", _ns);
            if (codeBaseNode == null)
            {
                codeBaseNode = _doc.CreateElement("codeBase", MsNamespace);
                codeBaseNode.SetAttribute("version", assemblyName.Version.ToString());
                codeBaseNode.SetAttribute("href", PathConstruction.GetRelativePath(_appDir, file));
                dependentAssemblyNode.AppendChild(codeBaseNode);
            }
        }
    }

    public void Dispose()
    {
        _doc.Save(_webConfigPath);
    }
}