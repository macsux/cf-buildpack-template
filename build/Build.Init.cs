
using Nuke.Common.IO;
using Nuke.Common.Tools.Git;
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
            Log.Information("Git repository doesn't exist - creating");
            GitTasks.Git("init", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
            GitTasks.Git("add version.json", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
            GitTasks.Git("commit -m version.json", workingDirectory: RootDirectory, logOutput: false, logInvocation: false);
            Log.Information("Added initial commit to make git versioning working");
            var thisFile = RootDirectory / "build" / "Build.Init.cs";
            thisFile.DeleteFile();
        }
    }
}