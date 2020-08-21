// Generated from https://github.com/nuke-build/nuke/blob/master/build/specifications/NerdbankGitVersioning.json

using JetBrains.Annotations;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Nuke.Common.Tools.NerdbankGitVersioning
{
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningTasks
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public static string NerdbankGitVersioningPath =>
            ToolPathResolver.TryGetEnvironmentExecutable("NERDBANKGITVERSIONING_EXE") ??
            ToolPathResolver.GetPackageExecutable("nbgv", "nbgv.dll");
        public static Action<OutputType, string> NerdbankGitVersioningLogger { get; set; } = ProcessTasks.DefaultLogger;
        /// <summary>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        public static IReadOnlyCollection<Output> NerdbankGitVersioning(string arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Func<string, string> outputFilter = null)
        {
            var process = ProcessTasks.StartProcess(NerdbankGitVersioningPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, NerdbankGitVersioningLogger, outputFilter);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Prepares a project to have version stamps applied using Nerdbank.GitVersioning</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningInstallSettings.Path"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningInstallSettings.Version"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningInstall(NerdbankGitVersioningInstallSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningInstallSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Prepares a project to have version stamps applied using Nerdbank.GitVersioning</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningInstallSettings.Path"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningInstallSettings.Version"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningInstall(Configure<NerdbankGitVersioningInstallSettings> configurator)
        {
            return NerdbankGitVersioningInstall(configurator(new NerdbankGitVersioningInstallSettings()));
        }
        /// <summary>
        ///   <p>Prepares a project to have version stamps applied using Nerdbank.GitVersioning</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningInstallSettings.Path"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningInstallSettings.Version"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningInstallSettings Settings, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningInstall(CombinatorialConfigure<NerdbankGitVersioningInstallSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningInstall, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
        /// <summary>
        ///   <p>Gets the version information for a project</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;commitIsh&gt;</c> via <see cref="NerdbankGitVersioningGetVersionSettings.CommitIsh"/></li>
        ///     <li><c>-f</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Format"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Project"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Variable"/></li>
        ///   </ul>
        /// </remarks>
        public static (NerdbankGitVersioning Result, IReadOnlyCollection<Output> Output) NerdbankGitVersioningGetVersion(NerdbankGitVersioningGetVersionSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningGetVersionSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return (GetResult(process, toolSettings), process.Output);
        }
        /// <summary>
        ///   <p>Gets the version information for a project</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;commitIsh&gt;</c> via <see cref="NerdbankGitVersioningGetVersionSettings.CommitIsh"/></li>
        ///     <li><c>-f</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Format"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Project"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Variable"/></li>
        ///   </ul>
        /// </remarks>
        public static (NerdbankGitVersioning Result, IReadOnlyCollection<Output> Output) NerdbankGitVersioningGetVersion(Configure<NerdbankGitVersioningGetVersionSettings> configurator)
        {
            return NerdbankGitVersioningGetVersion(configurator(new NerdbankGitVersioningGetVersionSettings()));
        }
        /// <summary>
        ///   <p>Gets the version information for a project</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;commitIsh&gt;</c> via <see cref="NerdbankGitVersioningGetVersionSettings.CommitIsh"/></li>
        ///     <li><c>-f</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Format"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Project"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningGetVersionSettings.Variable"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningGetVersionSettings Settings, NerdbankGitVersioning Result, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningGetVersion(CombinatorialConfigure<NerdbankGitVersioningGetVersionSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningGetVersion, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
        /// <summary>
        ///   <p>Updates the version stamp that is applied to a project.</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;version&gt;</c> via <see cref="NerdbankGitVersioningSetVersionSettings.Version"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningSetVersionSettings.Project"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningSetVersion(NerdbankGitVersioningSetVersionSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningSetVersionSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Updates the version stamp that is applied to a project.</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;version&gt;</c> via <see cref="NerdbankGitVersioningSetVersionSettings.Version"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningSetVersionSettings.Project"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningSetVersion(Configure<NerdbankGitVersioningSetVersionSettings> configurator)
        {
            return NerdbankGitVersioningSetVersion(configurator(new NerdbankGitVersioningSetVersionSettings()));
        }
        /// <summary>
        ///   <p>Updates the version stamp that is applied to a project.</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;version&gt;</c> via <see cref="NerdbankGitVersioningSetVersionSettings.Version"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningSetVersionSettings.Project"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningSetVersionSettings Settings, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningSetVersion(CombinatorialConfigure<NerdbankGitVersioningSetVersionSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningSetVersion, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
        /// <summary>
        ///   <p>Creates a git tag to mark a version</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;versionOrRef&gt;</c> via <see cref="NerdbankGitVersioningTagSettings.VersionOrRef"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningTagSettings.Project"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningTag(NerdbankGitVersioningTagSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningTagSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Creates a git tag to mark a version</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;versionOrRef&gt;</c> via <see cref="NerdbankGitVersioningTagSettings.VersionOrRef"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningTagSettings.Project"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningTag(Configure<NerdbankGitVersioningTagSettings> configurator)
        {
            return NerdbankGitVersioningTag(configurator(new NerdbankGitVersioningTagSettings()));
        }
        /// <summary>
        ///   <p>Creates a git tag to mark a version</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;versionOrRef&gt;</c> via <see cref="NerdbankGitVersioningTagSettings.VersionOrRef"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningTagSettings.Project"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningTagSettings Settings, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningTag(CombinatorialConfigure<NerdbankGitVersioningTagSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningTag, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
        /// <summary>
        ///   <p>Gets the commit(s) that match a given version</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;versionOrRef&gt;</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.VersionOrRef"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.Project"/></li>
        ///     <li><c>-q</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningGetCommits(NerdbankGitVersioningGetCommitsSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningGetCommitsSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Gets the commit(s) that match a given version</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;versionOrRef&gt;</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.VersionOrRef"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.Project"/></li>
        ///     <li><c>-q</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningGetCommits(Configure<NerdbankGitVersioningGetCommitsSettings> configurator)
        {
            return NerdbankGitVersioningGetCommits(configurator(new NerdbankGitVersioningGetCommitsSettings()));
        }
        /// <summary>
        ///   <p>Gets the commit(s) that match a given version</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;versionOrRef&gt;</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.VersionOrRef"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.Project"/></li>
        ///     <li><c>-q</c> via <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningGetCommitsSettings Settings, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningGetCommits(CombinatorialConfigure<NerdbankGitVersioningGetCommitsSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningGetCommits, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
        /// <summary>
        ///   <p>Communicates with the ambient cloud build to set the build number and/or other cloud build variables.</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>-a</c> via <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></li>
        ///     <li><c>-c</c> via <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></li>
        ///     <li><c>-d</c> via <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningCloudSettings.Project"/></li>
        ///     <li><c>-s</c> via <see cref="NerdbankGitVersioningCloudSettings.CISystem"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningCloudSettings.Version"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningCloud(NerdbankGitVersioningCloudSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningCloudSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Communicates with the ambient cloud build to set the build number and/or other cloud build variables.</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>-a</c> via <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></li>
        ///     <li><c>-c</c> via <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></li>
        ///     <li><c>-d</c> via <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningCloudSettings.Project"/></li>
        ///     <li><c>-s</c> via <see cref="NerdbankGitVersioningCloudSettings.CISystem"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningCloudSettings.Version"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningCloud(Configure<NerdbankGitVersioningCloudSettings> configurator)
        {
            return NerdbankGitVersioningCloud(configurator(new NerdbankGitVersioningCloudSettings()));
        }
        /// <summary>
        ///   <p>Communicates with the ambient cloud build to set the build number and/or other cloud build variables.</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>-a</c> via <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></li>
        ///     <li><c>-c</c> via <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></li>
        ///     <li><c>-d</c> via <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningCloudSettings.Project"/></li>
        ///     <li><c>-s</c> via <see cref="NerdbankGitVersioningCloudSettings.CISystem"/></li>
        ///     <li><c>-v</c> via <see cref="NerdbankGitVersioningCloudSettings.Version"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningCloudSettings Settings, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningCloud(CombinatorialConfigure<NerdbankGitVersioningCloudSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningCloud, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
        /// <summary>
        ///   <p>Prepares a release by creating a release branch for the current version and adjusting the version on the current branch</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;tag&gt;</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.Tag"/></li>
        ///     <li><c>--nextVersion</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.NextVersion"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.Project"/></li>
        ///     <li><c>--versionIncrement</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.VersionOrRef"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningPrepareRelease(NerdbankGitVersioningPrepareReleaseSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new NerdbankGitVersioningPrepareReleaseSettings();
            var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>Prepares a release by creating a release branch for the current version and adjusting the version on the current branch</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;tag&gt;</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.Tag"/></li>
        ///     <li><c>--nextVersion</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.NextVersion"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.Project"/></li>
        ///     <li><c>--versionIncrement</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.VersionOrRef"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> NerdbankGitVersioningPrepareRelease(Configure<NerdbankGitVersioningPrepareReleaseSettings> configurator)
        {
            return NerdbankGitVersioningPrepareRelease(configurator(new NerdbankGitVersioningPrepareReleaseSettings()));
        }
        /// <summary>
        ///   <p>Prepares a release by creating a release branch for the current version and adjusting the version on the current branch</p>
        ///   <p>For more details, visit the <a href="https://github.com/AArnott/Nerdbank.GitVersioning">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>&lt;tag&gt;</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.Tag"/></li>
        ///     <li><c>--nextVersion</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.NextVersion"/></li>
        ///     <li><c>-p</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.Project"/></li>
        ///     <li><c>--versionIncrement</c> via <see cref="NerdbankGitVersioningPrepareReleaseSettings.VersionOrRef"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(NerdbankGitVersioningPrepareReleaseSettings Settings, IReadOnlyCollection<Output> Output)> NerdbankGitVersioningPrepareRelease(CombinatorialConfigure<NerdbankGitVersioningPrepareReleaseSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(NerdbankGitVersioningPrepareRelease, NerdbankGitVersioningLogger, degreeOfParallelism, completeOnFailure);
        }
    }
    #region NerdbankGitVersioningInstallSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningInstallSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the directory that should contain the version.json file. The default is the root of the git repo.
        /// </summary>
        public virtual string Path { get; internal set; }
        /// <summary>
        ///   The initial version to set. The default is '1.0-beta'.
        /// </summary>
        public virtual string Version { get; internal set; }
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("install")
              .Add("-p {value}", Path)
              .Add("-v {value}", Version);
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningGetVersionSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningGetVersionSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the project or project directory. The default is the current directory.
        /// </summary>
        public virtual string Project { get; internal set; }
        /// <summary>
        ///   The initial version to set. The default is '1.0-beta'.
        /// </summary>
        public virtual Format Format { get; internal set; }
        /// <summary>
        ///   The name of just one version property to print to stdout. When specified, the output is always in raw text. Useful in scripts.
        /// </summary>
        public virtual string Variable { get; internal set; }
        /// <summary>
        ///   The name of just one version property to print to stdout. When specified, the output is always in raw text. Useful in scripts.
        /// </summary>
        public virtual string CommitIsh { get; internal set; }
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("get-version")
              .Add("-p {value}", Project)
              .Add("-f {value}", Format)
              .Add("-v {value}", Variable)
              .Add("{value}", CommitIsh);
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningSetVersionSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningSetVersionSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.
        /// </summary>
        public virtual string Project { get; internal set; }
        /// <summary>
        ///   The version to set
        /// </summary>
        public virtual string Version { get; internal set; }
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("set-version")
              .Add("-p {value}", Project)
              .Add("{value}", Version);
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningTagSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningTagSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.
        /// </summary>
        public virtual string Project { get; internal set; }
        /// <summary>
        ///   The a.b.c[.d] version or git ref to be tagged. If not specified, HEAD is used.
        /// </summary>
        public virtual string VersionOrRef { get; internal set; }
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("tag")
              .Add("-p {value}", Project)
              .Add("{value}", VersionOrRef);
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningGetCommitsSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningGetCommitsSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.
        /// </summary>
        public virtual string Project { get; internal set; }
        /// <summary>
        ///   Use minimal output
        /// </summary>
        public virtual bool? Quite { get; internal set; }
        /// <summary>
        ///   The a.b.c[.d] version to find
        /// </summary>
        public virtual string VersionOrRef { get; internal set; }
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("get-commits")
              .Add("-p {value}", Project)
              .Add("-q", Quite)
              .Add("{value}", VersionOrRef);
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningCloudSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningCloudSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the project or project directory used to calculate the version. The default is the current directory. Ignored if the -v option is specified.
        /// </summary>
        public virtual string Project { get; internal set; }
        /// <summary>
        ///   The string to use for the cloud build number. If not specified, the computed version will be used.
        /// </summary>
        public virtual string Version { get; internal set; }
        /// <summary>
        ///   Force activation for a particular CI system. If not specified, auto-detection will be used
        /// </summary>
        public virtual CISystem CISystem { get; internal set; }
        /// <summary>
        ///   Defines ALL version variables as cloud build variables, with a "NBGV_" prefix.
        /// </summary>
        public virtual bool? AllVars { get; internal set; }
        /// <summary>
        ///   Defines a few common version variables as cloud build variables, with a "Git" prefix (e.g. GitBuildVersion, GitBuildVersionSimple, GitAssemblyInformationalVersion)
        /// </summary>
        public virtual bool? CommonVars { get; internal set; }
        /// <summary>
        ///   Additional cloud build variables to define.
        /// </summary>
        public virtual IReadOnlyDictionary<string, string> DefineVariable => DefineVariableInternal.AsReadOnly();
        internal Dictionary<string,string> DefineVariableInternal { get; set; } = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("cloud")
              .Add("-p {value}", Project)
              .Add("-v {value}", Version)
              .Add("-s {value}", CISystem)
              .Add("-a", AllVars)
              .Add("-c", CommonVars)
              .Add("-d {value}", DefineVariable, "{key}={value}");
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningPrepareReleaseSettings
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class NerdbankGitVersioningPrepareReleaseSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the NerdbankGitVersioning executable.
        /// </summary>
        public override string ToolPath => base.ToolPath ?? NerdbankGitVersioningTasks.NerdbankGitVersioningPath;
        public override Action<OutputType, string> CustomLogger => NerdbankGitVersioningTasks.NerdbankGitVersioningLogger;
        /// <summary>
        ///   The path to the project or project directory. The default is the current directory.
        /// </summary>
        public virtual string Project { get; internal set; }
        /// <summary>
        ///    The version to set for the current branch. If omitted, the next version is determined automatically by incrementing the current version
        /// </summary>
        public virtual string NextVersion { get; internal set; }
        /// <summary>
        ///   Overrides the 'versionIncrement' setting set in version.json for determining the next version of the current branch
        /// </summary>
        public virtual string VersionOrRef { get; internal set; }
        /// <summary>
        ///   The prerelease tag to apply on the release branch (if any). If not specified, any existing prerelease tag will be removed. The preceding hyphen may be omitted
        /// </summary>
        public virtual string Tag { get; internal set; }
        protected override Arguments ConfigureArguments(Arguments arguments)
        {
            arguments
              .Add("prepare-release")
              .Add("-p {value}", Project)
              .Add("--nextVersion {value}", NextVersion)
              .Add("--versionIncrement {value}", VersionOrRef)
              .Add("{value}", Tag);
            return base.ConfigureArguments(arguments);
        }
    }
    #endregion
    #region NerdbankGitVersioningInstallSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningInstallSettingsExtensions
    {
        #region Path
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningInstallSettings.Path"/></em></p>
        ///   <p>The path to the directory that should contain the version.json file. The default is the root of the git repo.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningInstallSettings SetPath(this NerdbankGitVersioningInstallSettings toolSettings, string path)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Path = path;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningInstallSettings.Path"/></em></p>
        ///   <p>The path to the directory that should contain the version.json file. The default is the root of the git repo.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningInstallSettings ResetPath(this NerdbankGitVersioningInstallSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Path = null;
            return toolSettings;
        }
        #endregion
        #region Version
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningInstallSettings.Version"/></em></p>
        ///   <p>The initial version to set. The default is '1.0-beta'.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningInstallSettings SetVersion(this NerdbankGitVersioningInstallSettings toolSettings, string version)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Version = version;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningInstallSettings.Version"/></em></p>
        ///   <p>The initial version to set. The default is '1.0-beta'.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningInstallSettings ResetVersion(this NerdbankGitVersioningInstallSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Version = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region NerdbankGitVersioningGetVersionSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningGetVersionSettingsExtensions
    {
        #region Project
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetVersionSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the current directory.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings SetProject(this NerdbankGitVersioningGetVersionSettings toolSettings, string project)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = project;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetVersionSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the current directory.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings ResetProject(this NerdbankGitVersioningGetVersionSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = null;
            return toolSettings;
        }
        #endregion
        #region Format
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetVersionSettings.Format"/></em></p>
        ///   <p>The initial version to set. The default is '1.0-beta'.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings SetFormat(this NerdbankGitVersioningGetVersionSettings toolSettings, Format format)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Format = format;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetVersionSettings.Format"/></em></p>
        ///   <p>The initial version to set. The default is '1.0-beta'.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings ResetFormat(this NerdbankGitVersioningGetVersionSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Format = null;
            return toolSettings;
        }
        #endregion
        #region Variable
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetVersionSettings.Variable"/></em></p>
        ///   <p>The name of just one version property to print to stdout. When specified, the output is always in raw text. Useful in scripts.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings SetVariable(this NerdbankGitVersioningGetVersionSettings toolSettings, string variable)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Variable = variable;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetVersionSettings.Variable"/></em></p>
        ///   <p>The name of just one version property to print to stdout. When specified, the output is always in raw text. Useful in scripts.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings ResetVariable(this NerdbankGitVersioningGetVersionSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Variable = null;
            return toolSettings;
        }
        #endregion
        #region CommitIsh
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetVersionSettings.CommitIsh"/></em></p>
        ///   <p>The name of just one version property to print to stdout. When specified, the output is always in raw text. Useful in scripts.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings SetCommitIsh(this NerdbankGitVersioningGetVersionSettings toolSettings, string commitIsh)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommitIsh = commitIsh;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetVersionSettings.CommitIsh"/></em></p>
        ///   <p>The name of just one version property to print to stdout. When specified, the output is always in raw text. Useful in scripts.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetVersionSettings ResetCommitIsh(this NerdbankGitVersioningGetVersionSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommitIsh = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region NerdbankGitVersioningSetVersionSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningSetVersionSettingsExtensions
    {
        #region Project
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningSetVersionSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningSetVersionSettings SetProject(this NerdbankGitVersioningSetVersionSettings toolSettings, string project)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = project;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningSetVersionSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningSetVersionSettings ResetProject(this NerdbankGitVersioningSetVersionSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = null;
            return toolSettings;
        }
        #endregion
        #region Version
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningSetVersionSettings.Version"/></em></p>
        ///   <p>The version to set</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningSetVersionSettings SetVersion(this NerdbankGitVersioningSetVersionSettings toolSettings, string version)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Version = version;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningSetVersionSettings.Version"/></em></p>
        ///   <p>The version to set</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningSetVersionSettings ResetVersion(this NerdbankGitVersioningSetVersionSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Version = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region NerdbankGitVersioningTagSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningTagSettingsExtensions
    {
        #region Project
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningTagSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningTagSettings SetProject(this NerdbankGitVersioningTagSettings toolSettings, string project)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = project;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningTagSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningTagSettings ResetProject(this NerdbankGitVersioningTagSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = null;
            return toolSettings;
        }
        #endregion
        #region VersionOrRef
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningTagSettings.VersionOrRef"/></em></p>
        ///   <p>The a.b.c[.d] version or git ref to be tagged. If not specified, HEAD is used.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningTagSettings SetVersionOrRef(this NerdbankGitVersioningTagSettings toolSettings, string versionOrRef)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.VersionOrRef = versionOrRef;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningTagSettings.VersionOrRef"/></em></p>
        ///   <p>The a.b.c[.d] version or git ref to be tagged. If not specified, HEAD is used.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningTagSettings ResetVersionOrRef(this NerdbankGitVersioningTagSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.VersionOrRef = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region NerdbankGitVersioningGetCommitsSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningGetCommitsSettingsExtensions
    {
        #region Project
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetCommitsSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings SetProject(this NerdbankGitVersioningGetCommitsSettings toolSettings, string project)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = project;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetCommitsSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the root directory of the repo that spans the current directory, or an existing version.json file, if applicable.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings ResetProject(this NerdbankGitVersioningGetCommitsSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = null;
            return toolSettings;
        }
        #endregion
        #region Quite
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></em></p>
        ///   <p>Use minimal output</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings SetQuite(this NerdbankGitVersioningGetCommitsSettings toolSettings, bool? quite)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Quite = quite;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></em></p>
        ///   <p>Use minimal output</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings ResetQuite(this NerdbankGitVersioningGetCommitsSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Quite = null;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Enables <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></em></p>
        ///   <p>Use minimal output</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings EnableQuite(this NerdbankGitVersioningGetCommitsSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Quite = true;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Disables <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></em></p>
        ///   <p>Use minimal output</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings DisableQuite(this NerdbankGitVersioningGetCommitsSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Quite = false;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Toggles <see cref="NerdbankGitVersioningGetCommitsSettings.Quite"/></em></p>
        ///   <p>Use minimal output</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings ToggleQuite(this NerdbankGitVersioningGetCommitsSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Quite = !toolSettings.Quite;
            return toolSettings;
        }
        #endregion
        #region VersionOrRef
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningGetCommitsSettings.VersionOrRef"/></em></p>
        ///   <p>The a.b.c[.d] version to find</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings SetVersionOrRef(this NerdbankGitVersioningGetCommitsSettings toolSettings, string versionOrRef)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.VersionOrRef = versionOrRef;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningGetCommitsSettings.VersionOrRef"/></em></p>
        ///   <p>The a.b.c[.d] version to find</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningGetCommitsSettings ResetVersionOrRef(this NerdbankGitVersioningGetCommitsSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.VersionOrRef = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region NerdbankGitVersioningCloudSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningCloudSettingsExtensions
    {
        #region Project
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningCloudSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory used to calculate the version. The default is the current directory. Ignored if the -v option is specified.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetProject(this NerdbankGitVersioningCloudSettings toolSettings, string project)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = project;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningCloudSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory used to calculate the version. The default is the current directory. Ignored if the -v option is specified.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ResetProject(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = null;
            return toolSettings;
        }
        #endregion
        #region Version
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningCloudSettings.Version"/></em></p>
        ///   <p>The string to use for the cloud build number. If not specified, the computed version will be used.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetVersion(this NerdbankGitVersioningCloudSettings toolSettings, string version)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Version = version;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningCloudSettings.Version"/></em></p>
        ///   <p>The string to use for the cloud build number. If not specified, the computed version will be used.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ResetVersion(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Version = null;
            return toolSettings;
        }
        #endregion
        #region CISystem
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningCloudSettings.CISystem"/></em></p>
        ///   <p>Force activation for a particular CI system. If not specified, auto-detection will be used</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetCISystem(this NerdbankGitVersioningCloudSettings toolSettings, CISystem cisystem)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CISystem = cisystem;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningCloudSettings.CISystem"/></em></p>
        ///   <p>Force activation for a particular CI system. If not specified, auto-detection will be used</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ResetCISystem(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CISystem = null;
            return toolSettings;
        }
        #endregion
        #region AllVars
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></em></p>
        ///   <p>Defines ALL version variables as cloud build variables, with a "NBGV_" prefix.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetAllVars(this NerdbankGitVersioningCloudSettings toolSettings, bool? allVars)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.AllVars = allVars;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></em></p>
        ///   <p>Defines ALL version variables as cloud build variables, with a "NBGV_" prefix.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ResetAllVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.AllVars = null;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Enables <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></em></p>
        ///   <p>Defines ALL version variables as cloud build variables, with a "NBGV_" prefix.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings EnableAllVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.AllVars = true;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Disables <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></em></p>
        ///   <p>Defines ALL version variables as cloud build variables, with a "NBGV_" prefix.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings DisableAllVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.AllVars = false;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Toggles <see cref="NerdbankGitVersioningCloudSettings.AllVars"/></em></p>
        ///   <p>Defines ALL version variables as cloud build variables, with a "NBGV_" prefix.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ToggleAllVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.AllVars = !toolSettings.AllVars;
            return toolSettings;
        }
        #endregion
        #region CommonVars
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></em></p>
        ///   <p>Defines a few common version variables as cloud build variables, with a "Git" prefix (e.g. GitBuildVersion, GitBuildVersionSimple, GitAssemblyInformationalVersion)</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetCommonVars(this NerdbankGitVersioningCloudSettings toolSettings, bool? commonVars)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommonVars = commonVars;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></em></p>
        ///   <p>Defines a few common version variables as cloud build variables, with a "Git" prefix (e.g. GitBuildVersion, GitBuildVersionSimple, GitAssemblyInformationalVersion)</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ResetCommonVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommonVars = null;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Enables <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></em></p>
        ///   <p>Defines a few common version variables as cloud build variables, with a "Git" prefix (e.g. GitBuildVersion, GitBuildVersionSimple, GitAssemblyInformationalVersion)</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings EnableCommonVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommonVars = true;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Disables <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></em></p>
        ///   <p>Defines a few common version variables as cloud build variables, with a "Git" prefix (e.g. GitBuildVersion, GitBuildVersionSimple, GitAssemblyInformationalVersion)</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings DisableCommonVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommonVars = false;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Toggles <see cref="NerdbankGitVersioningCloudSettings.CommonVars"/></em></p>
        ///   <p>Defines a few common version variables as cloud build variables, with a "Git" prefix (e.g. GitBuildVersion, GitBuildVersionSimple, GitAssemblyInformationalVersion)</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ToggleCommonVars(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.CommonVars = !toolSettings.CommonVars;
            return toolSettings;
        }
        #endregion
        #region DefineVariable
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/> to a new dictionary</em></p>
        ///   <p>Additional cloud build variables to define.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetDefineVariable(this NerdbankGitVersioningCloudSettings toolSettings, IDictionary<string, string> defineVariable)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.DefineVariableInternal = defineVariable.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Clears <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></em></p>
        ///   <p>Additional cloud build variables to define.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings ClearDefineVariable(this NerdbankGitVersioningCloudSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.DefineVariableInternal.Clear();
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Adds a new key-value-pair <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></em></p>
        ///   <p>Additional cloud build variables to define.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings AddDefineVariable(this NerdbankGitVersioningCloudSettings toolSettings, string defineVariableKey, string defineVariableValue)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.DefineVariableInternal.Add(defineVariableKey, defineVariableValue);
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Removes a key-value-pair from <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></em></p>
        ///   <p>Additional cloud build variables to define.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings RemoveDefineVariable(this NerdbankGitVersioningCloudSettings toolSettings, string defineVariableKey)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.DefineVariableInternal.Remove(defineVariableKey);
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Sets a key-value-pair in <see cref="NerdbankGitVersioningCloudSettings.DefineVariable"/></em></p>
        ///   <p>Additional cloud build variables to define.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningCloudSettings SetDefineVariable(this NerdbankGitVersioningCloudSettings toolSettings, string defineVariableKey, string defineVariableValue)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.DefineVariableInternal[defineVariableKey] = defineVariableValue;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region NerdbankGitVersioningPrepareReleaseSettingsExtensions
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class NerdbankGitVersioningPrepareReleaseSettingsExtensions
    {
        #region Project
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningPrepareReleaseSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the current directory.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings SetProject(this NerdbankGitVersioningPrepareReleaseSettings toolSettings, string project)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = project;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningPrepareReleaseSettings.Project"/></em></p>
        ///   <p>The path to the project or project directory. The default is the current directory.</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings ResetProject(this NerdbankGitVersioningPrepareReleaseSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Project = null;
            return toolSettings;
        }
        #endregion
        #region NextVersion
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningPrepareReleaseSettings.NextVersion"/></em></p>
        ///   <p> The version to set for the current branch. If omitted, the next version is determined automatically by incrementing the current version</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings SetNextVersion(this NerdbankGitVersioningPrepareReleaseSettings toolSettings, string nextVersion)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NextVersion = nextVersion;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningPrepareReleaseSettings.NextVersion"/></em></p>
        ///   <p> The version to set for the current branch. If omitted, the next version is determined automatically by incrementing the current version</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings ResetNextVersion(this NerdbankGitVersioningPrepareReleaseSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NextVersion = null;
            return toolSettings;
        }
        #endregion
        #region VersionOrRef
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningPrepareReleaseSettings.VersionOrRef"/></em></p>
        ///   <p>Overrides the 'versionIncrement' setting set in version.json for determining the next version of the current branch</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings SetVersionOrRef(this NerdbankGitVersioningPrepareReleaseSettings toolSettings, string versionOrRef)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.VersionOrRef = versionOrRef;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningPrepareReleaseSettings.VersionOrRef"/></em></p>
        ///   <p>Overrides the 'versionIncrement' setting set in version.json for determining the next version of the current branch</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings ResetVersionOrRef(this NerdbankGitVersioningPrepareReleaseSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.VersionOrRef = null;
            return toolSettings;
        }
        #endregion
        #region Tag
        /// <summary>
        ///   <p><em>Sets <see cref="NerdbankGitVersioningPrepareReleaseSettings.Tag"/></em></p>
        ///   <p>The prerelease tag to apply on the release branch (if any). If not specified, any existing prerelease tag will be removed. The preceding hyphen may be omitted</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings SetTag(this NerdbankGitVersioningPrepareReleaseSettings toolSettings, string tag)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Tag = tag;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="NerdbankGitVersioningPrepareReleaseSettings.Tag"/></em></p>
        ///   <p>The prerelease tag to apply on the release branch (if any). If not specified, any existing prerelease tag will be removed. The preceding hyphen may be omitted</p>
        /// </summary>
        [Pure]
        public static NerdbankGitVersioningPrepareReleaseSettings ResetTag(this NerdbankGitVersioningPrepareReleaseSettings toolSettings)
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Tag = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
    #region Format
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [Serializable]
    [ExcludeFromCodeCoverage]
    [TypeConverter(typeof(TypeConverter<Format>))]
    public partial class Format : Enumeration
    {
        public static Format Text = (Format) "Text";
        public static Format Json = (Format) "Json";
        public static explicit operator Format(string value)
        {
            return new Format { Value = value };
        }
    }
    #endregion
    #region CISystem
    /// <summary>
    ///   Used within <see cref="NerdbankGitVersioningTasks"/>.
    /// </summary>
    [PublicAPI]
    [Serializable]
    [ExcludeFromCodeCoverage]
    [TypeConverter(typeof(TypeConverter<CISystem>))]
    public partial class CISystem : Enumeration
    {
        public static CISystem AppVeyor = (CISystem) "AppVeyor";
        public static CISystem VisualStudioTeamServices = (CISystem) "VisualStudioTeamServices";
        public static CISystem GitHubActions = (CISystem) "GitHubActions";
        public static CISystem TeamCity = (CISystem) "TeamCity";
        public static CISystem AtlassianBamboo = (CISystem) "AtlassianBamboo";
        public static CISystem Jenkins = (CISystem) "Jenkins";
        public static CISystem GitLab = (CISystem) "GitLab";
        public static CISystem Travis = (CISystem) "Travis";
        public static explicit operator CISystem(string value)
        {
            return new CISystem { Value = value };
        }
    }
    #endregion
}
