using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CloudFoundry.Buildpack.V2.Build;
using CloudFoundry.Buildpack.V2.Manifest;
using ICSharpCode.SharpZipLib.Zip;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CloudFoundry.Buildpack.V2.Build;

public interface IMakeOfflineBuildpack : IBuildpackBase
{
    [Parameter("Location of the buildpack to be turned into offline buildpack")]
    string BuildpackLocation => TryGetValue(() => BuildpackLocation);

    Target MakeOffline => _ => _
        .Requires(() => BuildpackLocation)
        .Executes(() =>
        {
            DoMakeOffline(BuildpackLocation);
        });

    void DoMakeOffline(string buildpackLocation, string? offlineBuildpackName = null)
    {
        var remoteLocation = buildpackLocation!;
            AbsolutePath inputBuildpackLocation;
            string buildpackFilename;
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var md5 = MD5.Create();
            try
            {
                inputBuildpackLocation = buildpackLocation!;
                buildpackFilename = Path.GetFileName(buildpackLocation);
            }
            catch (ArgumentException)
            {
                if (!remoteLocation.StartsWith("http"))
                    throw;
                buildpackFilename = new Uri(remoteLocation).Segments.Last().Trim('/');
                if (string.IsNullOrEmpty(buildpackFilename))
                {
                    buildpackFilename = "offline-buildpack.zip";
                }

                inputBuildpackLocation = ArtifactsDirectory / buildpackFilename;
                HttpTasks.HttpDownloadFile(remoteLocation, inputBuildpackLocation, clientConfigurator: client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    return client;
                });
            }

            var offlineBuildpackFileName = offlineBuildpackName == null ? $"{Path.GetFileNameWithoutExtension(buildpackFilename)}-offline.zip" : $"{offlineBuildpackName}.zip";
            var offlineBuildpackLocation = ArtifactsDirectory / offlineBuildpackFileName;

            using var inputStream = new ZipInputStream(File.Open(inputBuildpackLocation, FileMode.Open));
            using var outputStream = new ZipOutputStream(File.Open(offlineBuildpackLocation, FileMode.Create));
            using var streamWriter = new StreamWriter(outputStream);
            using var streamReader = new StreamReader(inputStream);
            outputStream.SetLevel(9);
            ZipEntry entry;
            while ((entry = inputStream.GetNextEntry()) != null)
            {
                var outEntry = (ZipEntry)entry.Clone();
                outEntry.HostSystem = (int)HostSystemID.Unix;
                var permissions = ZipEntryAttributes.ReadOwner |
                                  ZipEntryAttributes.ReadOther |
                                  ZipEntryAttributes.ReadGroup;
                if (outEntry.Name is "bin/detect" or "bin/release" or "bin/supply" or "bin/compile" or "bin/finalize" || outEntry.Name.EndsWith(".sh"))
                {
                    permissions |=
                        ZipEntryAttributes.ExecuteOwner |
                        ZipEntryAttributes.ExecuteOther |
                        ZipEntryAttributes.ExecuteGroup;
                }

                outEntry.ExternalFileAttributes = (int)permissions << 16;

                outputStream.PutNextEntry(outEntry);
                if (outEntry.Name == "manifest.yml")
                {
                    var inputManifestYaml = streamReader.ReadToEnd();
                    var manifest = deserializer.Deserialize<BuildpackManifest>(inputManifestYaml);
           
                    var dependencies = manifest.Dependencies;
                    var dependencyCache = ArtifactsDirectory / ".cache";
                    foreach(var dep in dependencies)
                    {
                
                        var hash = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(dep.Uri))).ToLower().Replace("-", "");
                        var fileName = new Uri(dep.Uri).Segments.Last();
                        if (!Directory.Exists(dependencyCache / hash))
                        {
                            Log.Logger.Information("Downloading dependency '{Name}' from {Uri}", dep.Name, dep.Uri);
                            HttpTasks.HttpDownloadFile(dep.Uri, dependencyCache / hash / fileName);
                        }
                        // FileSystemTasks.CopyDirectoryRecursively(dependencyCache / hash, workDir / "dependencies" / hash);
                        dep.File = $"dependencies/{hash}/{fileName}";
                    }
                    var outputManifest = serializer.Serialize(manifest);
                    streamWriter.Write(outputManifest);
                    streamWriter.Flush();
                    foreach (var dep in dependencies)
                    {
                        using var depStream = new FileStream(ArtifactsDirectory / ".cache" / Regex.Replace(dep.File, "^dependencies/", ""), FileMode.Open);
                        outEntry = new ZipEntry(dep.File);
                        outputStream.PutNextEntry(outEntry);
                        depStream.CopyTo(outputStream);
                    }
                }
                else
                {
                    inputStream.CopyTo(outputStream);
                }
            }
    }
}

