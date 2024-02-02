using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using FileMode = System.IO.FileMode;
using Project = Nuke.Common.ProjectModel.Project;
using ZipFile = System.IO.Compression.ZipFile;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

[assembly: InternalsVisibleTo("MyBuildpackTests")]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    static Build()
    {
        Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "true");
    }

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    [Flags]
    public enum StackType
    {
        Windows = 1,
        Linux = 2
    }

    public static int Main () => Execute<Build>(x => x.Publish);
    const string BuildpackProjectName = "MyBuildpack";
    Project BuildpackProject => Solution.GetAllProjects(BuildpackProjectName).Single();
    string GetPackageZipName(string runtime) => $"{BuildpackProjectName}-{runtime}-{GitVersion.SemVer1}.zip";

    [Parameter("Target CF stack type - 'windows' or 'linux'. Determines buildpack runtime (Framework or Core). Default is both")]
    readonly StackType Stack = StackType.Windows | StackType.Linux;
    
    [Parameter("GitHub personal access token with access to the repo")]
    string GitHubToken;

    IEnumerable<PublishTarget> PublishCombinations
    {
        get
        {
            if (Stack.HasFlag(StackType.Windows))
                yield return new PublishTarget {Framework = "net48", Runtime = "win-x64"};
            if (Stack.HasFlag(StackType.Linux))
                yield return new PublishTarget {Framework = "net8.0", Runtime = "linux-x64"};
        }
    }

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [NerdbankGitVersioning(UpdateBuildNumber = true)] readonly NerdbankGitVersioning GitVersion;

    string Configuration = "Debug";
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    
    string[] LifecycleHooks = {"detect", "supply", "release", "finalize"};

    Target Clean => _ => _
        .Description("Cleans up **/bin and **/obj folders")
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
        });
    
    Target Publish => _ => _
        .Description("Packages buildpack in Cloud Foundry expected format into /artifacts directory")
        .DependsOn(Clean)
        .Executes(() =>
        {
            foreach (var publishCombination in PublishCombinations)
            {
                var extension = publishCombination.Runtime.StartsWith("win") ? ".exe" : "";
                var framework = publishCombination.Framework;
                var runtime = publishCombination.Runtime;
                var packageZipName = GetPackageZipName(runtime);
                var workDirectory = TemporaryDirectory / "pack";
                workDirectory.CreateOrCleanDirectory();
                var buildpackProject = Solution.GetAllProjects(BuildpackProjectName).Single();
                if(buildpackProject == null)
                    throw new Exception($"Unable to find project called {BuildpackProjectName} in solution {Solution.Name}");
                var publishDirectory = buildpackProject.Directory / "bin" / Configuration / framework / runtime / "publish";
                var workBinDirectory = workDirectory / "bin";
                var workLibDirectory = workDirectory / "lib";


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

                CopyDirectoryRecursively(publishDirectory, workBinDirectory, DirectoryExistsPolicy.Merge);
                var supplyExecutable = workBinDirectory / $"supply{extension}";
                RenameFile(workBinDirectory / $"buildpack{extension}", supplyExecutable);
                if (publishCombination.Runtime.StartsWith("win"))
                {
                    CopyFile(supplyExecutable, workBinDirectory / "detect.exe");
                    CopyFile(supplyExecutable, workBinDirectory / "finalize.exe");
                    CopyFile(supplyExecutable, workBinDirectory / "release.exe");
                    CopyFile(supplyExecutable, workBinDirectory / "prestartup.exe");
                }
                var tempZipFile = TemporaryDirectory / packageZipName;
                ZipFile.CreateFromDirectory(workDirectory, tempZipFile, CompressionLevel.NoCompression, false);
                if (publishCombination.Runtime.StartsWith("linux"))
                {
                    MakeLinuxBuildpack(tempZipFile);
                    // MakeFilesInZipUnixExecutable(tempZipFile);
                }
                
                
                MoveFileToDirectory(tempZipFile, ArtifactsDirectory, FileExistsPolicy.Overwrite);
                Log.Information($"Package -> {ArtifactsDirectory / packageZipName}");
            }
        });

    void MakeLinuxBuildpack(AbsolutePath zipFile)
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
                var entry = new ZipEntry(name) {HostSystem = (int) HostSystemID.Unix};
                entry.ExternalFileAttributes = (int)(ZipEntryAttributes.SymbolicLink) << 16;
                streamWriter.Write(originalFile);
                output.PutNextEntry(entry);
            }
            while ((entry = input.GetNextEntry()) != null)
            {
                var outEntry = new ZipEntry(entry.Name) {HostSystem = (int) HostSystemID.Unix};
                var entryAttributes = entry.IsDirectory ? ZipEntryAttributes.Directory : ZipEntryAttributes.Regular;
                if (entry.Name == "bin/buildpack")
                {
                    outEntry = new ZipEntry("bin/supply");
                    entryAttributes |= ZipEntryAttributes.ReadOwner |
                                       ZipEntryAttributes.ReadOther |
                                       ZipEntryAttributes.ReadGroup |
                                       ZipEntryAttributes.ExecuteOwner |
                                       ZipEntryAttributes.ExecuteOther |
                                       ZipEntryAttributes.ExecuteGroup |
                                       ZipEntryAttributes.Regular;
                }
                outEntry.ExternalFileAttributes = (int)(entryAttributes) << 16; // https://unix.stackexchange.com/questions/14705/the-zip-formats-external-file-attribute
                
                output.PutNextEntry(outEntry);
                input.CopyTo(output);
            }
            AddSymLink("bin/detect", "bin/supply");
            AddSymLink("bin/finalize", "bin/supply");
            AddSymLink("bin/release", "bin/supply");
            AddSymLink("bin/prestartup", "bin/supply");
            output.Finish();
            output.Flush();
        }

        zipFile.DeleteFile();
        RenameFile(tmpFileName,zipFile, FileExistsPolicy.Overwrite);
    }

    Target Release => _ => _
        .Description("Creates a GitHub release (or amends existing) and uploads buildpack artifact")
        .DependsOn(Publish)
        .Requires(() => GitHubToken)
        .Executes(async () =>
        {
            foreach (var publishCombination in PublishCombinations)
            {
                var runtime = publishCombination.Runtime;
                var packageZipName = GetPackageZipName(runtime);
                if (!GitRepository.IsGitHubRepository())
                    throw new Exception("Only supported when git repo remote is github");
    
                var client = new GitHubClient(new ProductHeaderValue(BuildpackProjectName))
                {
                    Credentials = new Credentials(GitHubToken, AuthenticationType.Bearer)
                };
                var gitIdParts = GitRepository.Identifier.Split("/");
                var owner = gitIdParts[0];
                var repoName = gitIdParts[1];
    
                var releaseName = $"v{GitVersion.SemVer1}";
                Release release;
                try
                {
                    release = await client.Repository.Release.Get(owner, repoName, releaseName);
                }
                catch (NotFoundException)
                {
                    var newRelease = new NewRelease(releaseName)
                    {
                        Name = releaseName,
                        Draft = false,
                        Prerelease = false
                    };
                    release = await client.Repository.Release.Create(owner, repoName, newRelease);
                }
    
                var existingAsset = release.Assets.FirstOrDefault(x => x.Name == packageZipName);
                if (existingAsset != null)
                {
                    await client.Repository.Release.DeleteAsset(owner, repoName, existingAsset.Id);
                }
    
                var zipPackageLocation = ArtifactsDirectory / packageZipName;
                var stream = File.OpenRead(zipPackageLocation);
                var releaseAssetUpload = new ReleaseAssetUpload(packageZipName, "application/zip", stream, TimeSpan.FromHours(1));
                var releaseAsset = await client.Repository.Release.UploadAsset(release, releaseAssetUpload);
    
                Log.Information($"Buildpack URL: {releaseAsset.BrowserDownloadUrl}");
            }
        });

    // Target Detect => _ => _
    //     .Description("Invokes buildpack 'detect' lifecycle event")
    //     .Requires(() => ApplicationDirectory)
    //     .Executes(() =>
    //     {
    //         try
    //         {
    //             DotNetRun(s => s
    //                 .SetProjectFile(Solution.GetProject("Lifecycle.Detect").Path)
    //                 .SetApplicationArguments(ApplicationDirectory)
    //                 .SetConfiguration(Configuration)
    //                 .SetFramework("netcoreapp3.1"));
    //             Logger.Block("Detect returned 'true'");
    //         }
    //         catch (ProcessException)
    //         {
    //             Logger.Block("Detect returned 'false'");
    //         }
    //     });
    //
    // Target Supply => _ => _
    //     .Description("Invokes buildpack 'supply' lifecycle event")
    //     .Requires(() => ApplicationDirectory)
    //     .Executes(() =>
    //     {
    //         var home = (AbsolutePath)Path.GetTempPath() / Guid.NewGuid().ToString();
    //         var app = home / "app";
    //         var deps = home / "deps";
    //         var index = 0;
    //         var cache = home / "cache";
    //         CopyDirectoryRecursively(ApplicationDirectory, app);
    //
    //         DotNetRun(s => s
    //             .SetProjectFile(Solution.GetProject("Lifecycle.Supply").Path)
    //             .SetApplicationArguments($"{app} {cache} {deps} {index}")
    //             .SetConfiguration(Configuration)
    //             .SetFramework("netcoreapp3.1"));
    //         Logger.Block($"Buildpack applied. Droplet is available in {home}");
    //
    //     });

    void MakeFilesInZipUnixExecutable(AbsolutePath zipFile)
    {
        var tmpFileName = zipFile + ".tmp";
        using (var input = new ZipInputStream(File.Open(zipFile, FileMode.Open)))
        using (var output = new ZipOutputStream(File.Open(tmpFileName, FileMode.Create)))
        {
            output.SetLevel(9);
            ZipEntry entry;
		
            while ((entry = input.GetNextEntry()) != null)
            {
                var outEntry = new ZipEntry(entry.Name) {HostSystem = (int) HostSystemID.Unix};
                var entryAttributes =  
                    ZipEntryAttributes.ReadOwner | 
                    ZipEntryAttributes.ReadOther | 
                    ZipEntryAttributes.ReadGroup |
                    ZipEntryAttributes.ExecuteOwner | 
                    ZipEntryAttributes.ExecuteOther | 
                    ZipEntryAttributes.ExecuteGroup;
                entryAttributes = entryAttributes | (entry.IsDirectory ? ZipEntryAttributes.Directory : ZipEntryAttributes.Regular);
                outEntry.ExternalFileAttributes = (int) (entryAttributes) << 16; // https://unix.stackexchange.com/questions/14705/the-zip-formats-external-file-attribute
                
                output.PutNextEntry(outEntry);
                input.CopyTo(output);
            }
            output.Finish();
            output.Flush();
        }

        zipFile.DeleteFile();
        RenameFile(tmpFileName,zipFile, FileExistsPolicy.Overwrite);
    }
    
    
    // See reference: https://minnie.tuhs.org/cgi-bin/utree.pl?file=4.4BSD/usr/include/sys/stat.h
    // (values are in octal - use Convert.ToInt32("OCTALNUM", 8) to get values you see below
    [Flags]
    enum ZipEntryAttributes
    {
        ExecuteOther = 1,
        WriteOther = 2,
        ReadOther = 4,
	
        ExecuteGroup = 8,
        WriteGroup = 16,
        ReadGroup = 32,

        ExecuteOwner = 64,
        WriteOwner = 128,
        ReadOwner = 256,

        Sticky = 512, // S_ISVTX
        SetGroupIdOnExecution = 1024,
        SetUserIdOnExecution = 2048,

        //This is the file type constant of a block-oriented device file.
        NamedPipe = 4096,
        CharacterSpecial = 8192,
        Directory = 16384,
        Block = 24576,
        Regular = 32768,
        SymbolicLink = 40960,
        Socket = 49152
	
    }
    class PublishTarget
    {
        public string Framework { get; set; }
        public string Runtime { get; set; }
    }
}
