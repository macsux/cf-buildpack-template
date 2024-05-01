using CloudFoundry.Buildpack.V2;
using CloudFoundry.Buildpack.V2.MyBuildpack;

return BuildpackHost.Create<MyBuildpackBuildpack>().Run();