
using CloudFoundry.Buildpack.V2.Build;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Git;
using Octokit;
using Serilog;

partial class Build
{
    // ensure git repo is initialized if we try to build immediately after creating template
    protected override void OnBuildCreated()
    {
        try
        {
            GitTasks.Git("status", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
        }
        catch (Exception)
        {
            Log.Information("First build - Initializing workspace");
            Log.Information("Git repository doesn't exist - creating");
            GitTasks.Git("init", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
            GitTasks.Git("add version.json", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
            GitTasks.Git("commit -m version.json", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
            Log.Information("Added initial commit to make git versioning working");
            
            var offlineBuildpackMaker = ((IMakeOfflineBuildpack)this);
            offlineBuildpackMaker.DoMakeOffline(FindLatestBuildpack("cloudfoundry", "dotnet-core-buildpack", "cflinuxfs4"), "dotnet-core-buildpack");
            offlineBuildpackMaker.DoMakeOffline(FindLatestBuildpack("cloudfoundry", "staticfile-buildpack", "cflinuxfs4"), "staticfile-buildpack");
            offlineBuildpackMaker.DoMakeOffline(FindLatestBuildpack("cloudfoundry", "hwc-buildpack", "windows"), "hwc-buildpack");
            var thisFile = RootDirectory / "build" / "Build.Init.cs";
            thisFile.DeleteFile();
        }
    }

    string FindLatestBuildpack(string owner, string repo, string stack)
    {
        var client = new GitHubClient(new ProductHeaderValue(BuildpackProjectName));
        var latestRelease = Task.Run(() => client.Repository.Release.GetLatest(owner, repo)).Result;
        var asset = latestRelease.Assets.First(x => x.Name.Contains(stack) && x.Name.EndsWith(".zip"));
        Log.Logger.Information("Downloading buildpack {Repo} from {Url}", repo, asset.BrowserDownloadUrl);
        return asset.BrowserDownloadUrl;
    }
}