﻿using JetBrains.Annotations;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2.Testing;

[PublicAPI]
public class LaunchContext
{
    internal AbsolutePath? RootDirectory { get; set; }
    public AbsolutePath? LifecycleDirectory { get; set; }
    public AbsolutePath? DropletDirectory { get; }
    public CloudFoundryStack Stack { get; set; }
    public AbsolutePath ApplicationDirectory => DropletDirectory / "app";
    public AbsolutePath DependenciesDirectory => DropletDirectory / "deps";
    public AbsolutePath ProfileDDirectory => DropletDirectory / "profile.d";
    internal LaunchContext(AbsolutePath dropletDirectory, CloudFoundryStack stack)
    {
        RootDirectory = DirectoryHelper.RootDirectory;
        LifecycleDirectory = RootDirectory / "lifecycle";
        DropletDirectory = dropletDirectory;
    }

    public static LaunchContext FromDropletDirectory(AbsolutePath dropletDirectory, CloudFoundryStack stack) => new (dropletDirectory, stack);
}