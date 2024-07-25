using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using CloudFoundry.Buildpack.V2.Manifest;
using ICSharpCode.SharpZipLib.Zip;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using FileMode = System.IO.FileMode;
using Project = Nuke.Common.ProjectModel.Project;
using ZipFile = System.IO.Compression.ZipFile;
namespace CloudFoundry.Buildpack.V2.Build;

public interface IPublishBuildpack : IBuildpackBase
{
    Target Publish => _ => _
        .Executes(() =>
        {
            foreach (var publishCombination in PublishCombinations)
            {
                var framework = publishCombination.Framework;
                var runtime = publishCombination.Runtime;

                DotNetPublish(s => s
                    .SetProject(BuildpackProject.Path)
                    .SetConfiguration(Configuration)
                    .EnableSelfContained()
                    .SetFramework(framework)
                    .SetRuntime(runtime)
                    .SetAssemblyVersion(GitVersion.AssemblyVersion)
                    .SetFileVersion(GitVersion.AssemblyFileVersion)
                    .SetInformationalVersion(GitVersion.AssemblyInformationalVersion)
                );
            }
        });

    Target PublishBuildpack => _ => _
        .Description("Packages buildpack in Cloud Foundry expected format into /artifacts directory")
        .DependsOn(Publish, EnsureCleanWorkDirectory)
        .TryDependsOn<IAssemblyInject>(x => x.BuildInjectors)
        .Executes(() =>
        {
            foreach (var publishCombination in PublishCombinations)
            {
                var extension = publishCombination.Runtime.StartsWith("win") ? ".exe" : "";
                var framework = publishCombination.Framework;
                var runtime = publishCombination.Runtime;
                var buildpackRoot = WorkDirectory / runtime;
                var packageZipName = GetPackageZipName(runtime);
                var buildpackProject = Solution.GetAllProjects(BuildpackProjectName).Single();
                if (buildpackProject == null)
                    throw new Exception($"Unable to find project called {BuildpackProjectName} in solution {Solution.Name}");
                // string outputPath = ((string)(TemporaryDirectory / "bin" / publishCombination.Runtime)) + Path.DirectorySeparatorChar;
                //string outputPath = BuildpackProject.Directory / "bin" / Configuration / publishCombination.Runtime)) + Path.DirectorySeparatorChar;
                //var publishDirectory = (AbsolutePath)outputPath / "publish";
                var publishDirectory = buildpackProject.Directory / "bin" / Configuration / framework / runtime / "publish";

                // CopyFile(buildpackProject.Directory / "manifest.yml", buildpackRoot / "manifest.yml");
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                
                var manifest = deserializer.Deserialize<BuildpackManifest>(File.ReadAllText(buildpackProject.Directory / "manifest.yml"));
                var dependencies = manifest.Dependencies;
                manifest.Stack = publishCombination.Stack == StackType.Windows ? "windows" : "cflinuxfs4";
                var dependencyCache = ArtifactsDirectory / ".cache";
                var md5 = MD5.Create();
                foreach(var dep in dependencies.Where(x => x.Uri != null))
                {
                    // var name = dep.Uri ?? $"{dep.Name}-{dep.Version}";
                    var hash = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(dep.Uri))).ToLower().Replace("-", "");
                    var fileName = new Uri(dep.Uri).Segments.Last();
                    var cachedDependencyDirectory = dependencyCache / hash;
                    if (!Directory.Exists(cachedDependencyDirectory))
                    {
                        Log.Logger.Information("Downloading dependency '{Name}' from {Uri}", dep.Name, dep.Uri);
                        HttpTasks.HttpDownloadFile(dep.Uri, cachedDependencyDirectory / fileName);
                    }
                    CopyDirectoryRecursively(cachedDependencyDirectory, buildpackRoot / "dependencies" / hash);
                    dep.File = $"dependencies/{hash}/{fileName}";
                    dep.Sha256 ??= GetFileSha256(buildpackRoot / "dependencies" / hash / fileName);
                    
                }
                var outputManifest = serializer.Serialize(manifest);
                File.WriteAllText(buildpackRoot / "manifest.yml", outputManifest);
                
                // var outputManifest = serializer.Serialize(manifest);
                // var manifest = deserializer.Deserialize<BuildpackManifest>(File.ReadAllText(buildpackRoot / "manifest.yml"));
                // foreach (var dependency in manifest.Dependencies)
                // {
                //     manifest.Dependencies.
                // }
                //
                var workBinDirectory = buildpackRoot / "bin";
                var workLibDirectory = buildpackRoot / "lib";

                CopyDirectoryRecursively(publishDirectory, workBinDirectory, DirectoryExistsPolicy.Merge);
                var supplyExecutable = workBinDirectory / $"supply{extension}";
                RenameFile(workBinDirectory / $"buildpack{extension}", supplyExecutable);

                if (publishCombination.Runtime.StartsWith("win"))
                {
                    if (Configuration == BuildConfiguration.Final)
                    {
                        CopyFile(supplyExecutable, workBinDirectory / $"detect{extension}");
                        CopyFile(supplyExecutable, workBinDirectory / $"finalize{extension}");
                        CopyFile(supplyExecutable, workBinDirectory / $"release{extension}");
                    }

                    CopyFile(supplyExecutable, workBinDirectory / $"prestartup{extension}");
                }

                var tempZipFile = TemporaryDirectory / packageZipName;
                tempZipFile.DeleteFile();
                ZipFile.CreateFromDirectory(buildpackRoot, tempZipFile, CompressionLevel.NoCompression, false);
                if (publishCombination.Runtime.StartsWith("linux"))
                {
                    MakeLinuxBuildpack(tempZipFile);
                }

                MoveFileToDirectory(tempZipFile, ArtifactsDirectory, FileExistsPolicy.Overwrite);
                var latestDir = ArtifactsDirectory / "latest" / runtime;
                latestDir.CreateOrCleanDirectory();
                CopyFile(ArtifactsDirectory / tempZipFile.Name, ArtifactsDirectory / "latest" / runtime / "buildpack.zip", FileExistsPolicy.Overwrite);

                Log.Information("Package -> {Package}", ArtifactsDirectory / packageZipName);
            }

            DotNetRestore(_ => _.SetProjectFile(Solution.Path));
        });

    /// <summary>
    /// Allows overriding permissions inside buildpack zip entry. This is needed to make any files that buildpacks contribute executable inside container (equivalent of chmod +x <paramref name="fileName"/>)
    /// </summary>
    ZipEntryAttributes AddFilePermission(string fileName)
    {
        return 0;
    }

    private void MakeLinuxBuildpack(AbsolutePath zipFile)
    {
        var tmpFileName = zipFile + ".tmp";
        using (var input = new ZipInputStream(File.Open(zipFile, FileMode.Open)))
        using (var output = new ZipOutputStream(File.Open(tmpFileName, FileMode.Create)))
        {
            output.SetLevel(9);
            ZipEntry entry;
            var streamWriter = new StreamWriter(output);

            void AddSymLink(string name, string originalFile)
            {
                var entry = new ZipEntry(name) { HostSystem = (int)HostSystemID.Unix };
                entry.ExternalFileAttributes = (int)(ZipEntryAttributes.SymbolicLink |
                                                     ZipEntryAttributes.ReadOwner |
                                                     ZipEntryAttributes.ReadOther |
                                                     ZipEntryAttributes.ReadGroup |
                                                     ZipEntryAttributes.ExecuteOwner |
                                                     ZipEntryAttributes.ExecuteOther |
                                                     ZipEntryAttributes.ExecuteGroup) << 16;
                output.PutNextEntry(entry);
                streamWriter.Write(originalFile);
                streamWriter.Flush();
            }

            while ((entry = input.GetNextEntry()) != null)
            {
                var outEntry = new ZipEntry(entry.Name) { HostSystem = (int)HostSystemID.Unix };
                var permissions =  ZipEntryAttributes.ReadOwner |
                                   ZipEntryAttributes.ReadOther |
                                   ZipEntryAttributes.ReadGroup;
                if (outEntry.Name.EndsWith(".sh"))
                {
                    permissions |= ZipEntryAttributes.ExecuteOwner |
                                   ZipEntryAttributes.ExecuteOther |
                                   ZipEntryAttributes.ExecuteGroup;
                }
                var entryAttributes = permissions | AddFilePermission(outEntry.Name);
                // if (entry.Name == "bin/buildpack" || entry.Name == "bin/supply")
                if (Lifecycle.AllValues.Contains(Path.GetFileNameWithoutExtension(entry.Name)))
                {
                    // outEntry = new ZipEntry("bin/supply") {HostSystem = (int) HostSystemID.Unix};
                    entryAttributes = entryAttributes |
                                      ZipEntryAttributes.ExecuteOwner |
                                      ZipEntryAttributes.ExecuteOther |
                                      ZipEntryAttributes.ExecuteGroup;
                }

                entryAttributes = entryAttributes | (entry.IsDirectory ? ZipEntryAttributes.Directory : ZipEntryAttributes.Regular);
                outEntry.ExternalFileAttributes = (int)(entryAttributes) << 16; // https://unix.stackexchange.com/questions/14705/the-zip-formats-external-file-attribute

                output.PutNextEntry(outEntry);
                input.CopyTo(output);
            }

            if (Configuration == BuildConfiguration.Final)
            {
                AddSymLink("bin/detect", "supply");
                AddSymLink("bin/finalize", "supply");
                AddSymLink("bin/release", "supply");
            }

            AddSymLink("bin/prestartup", "supply");

            output.Finish();
            output.Flush();
        }

        zipFile.DeleteFile();
        RenameFile(tmpFileName, zipFile, FileExistsPolicy.Overwrite);
    }

    private static string GetFileSha256(AbsolutePath file)
    {
        using var stream = File.OpenRead(@"C:\projects\cf-buildpack-template\artifacts\.cache\e086b4658b85f113226290e1ffb9adc9\nginx-static_1.26.1_linux_x64_cflinuxfs4_02d0203c.tgz");
        var hex = Convert.ToHexString(SHA256.HashData(stream)).ToLower();
        return hex;
    }
    
}

