using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
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
                var publishWorkDirectory = WorkDirectory / runtime;
                var packageZipName = GetPackageZipName(runtime);
                var buildpackProject = Solution.GetAllProjects(BuildpackProjectName).Single();
                if (buildpackProject == null)
                    throw new Exception($"Unable to find project called {BuildpackProjectName} in solution {Solution.Name}");
                // string outputPath = ((string)(TemporaryDirectory / "bin" / publishCombination.Runtime)) + Path.DirectorySeparatorChar;
                //string outputPath = BuildpackProject.Directory / "bin" / Configuration / publishCombination.Runtime)) + Path.DirectorySeparatorChar;
                //var publishDirectory = (AbsolutePath)outputPath / "publish";
                var publishDirectory = buildpackProject.Directory / "bin" / Configuration / framework / runtime / "publish";

                var workBinDirectory = publishWorkDirectory / "bin";
                var workLibDirectory = publishWorkDirectory / "lib";

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
                ZipFile.CreateFromDirectory(publishWorkDirectory, tempZipFile, CompressionLevel.NoCompression, false);
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
                var entryAttributes = ZipEntryAttributes.ReadOwner |
                                      ZipEntryAttributes.ReadOther |
                                      ZipEntryAttributes.ReadGroup;
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
}

