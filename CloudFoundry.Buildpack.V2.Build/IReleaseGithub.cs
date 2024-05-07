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

public interface IReleaseGithub : IBuildpackBase
{
    [GitRepository] GitRepository GitRepository  => TryGetValue(() => GitRepository)!;
    [Parameter("GitHub personal access token with access to the repo")]
    string? GitHubToken => TryGetValue(() => GitHubToken);
    
    Target GithubRelease => _ => _
        .Description("Creates a GitHub release (or amends existing) and uploads buildpack artifact")
        .DependsOn<IPublishBuildpack>(x => x.PublishBuildpack)
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
}