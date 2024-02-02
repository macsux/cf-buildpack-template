using CloudFoundry.Buildpack.V2;
using CloudFoundry.Buildpack.V2.MyBuildpack;

BuildpackHost.Create<MyBuildpack>().Run();