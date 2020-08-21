This project offers simplified way of creating Cloud Foundry buildpacks that install HttpModules into .NET framework apps that run in IIS and use `hwc_buildpack`. 


## Quick Start

This is a template project that can be used by DotNet CLI to boostrap a new buildpack. 

1. Install the template into CLI via

    ```powershell
    dotnet new -i CloudFoundry.Buildpack.V2.HttpModule
    ```
2. Create new buildpack based on template
	```powershell
	dotnet new -i buildpack-httpmodule -n <MY_BUILDPACK_NAME>
	```
3. The embedded build scripts rely on Git to do versioning through the use of [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning). Initialize git repo and auto versioning:

   ```powershell
   .\initialize.ps1
   ```
4. Implement your logic by modifying `MyBuildpackHttpModule` class
5. Use provided `build.ps1` to invoke Nuke.Build targets that compile, package and publish the app to Git. See the list of all included targets and parameters by running
	```powershell
	.\build.ps1 --help
	```


## How to package

Simply runs `.\build.ps1` which will invoke default target that will compile and package the buildpack into `/artifacts` folder.


## How to Release

Use the embedded `Release` target to create and publish buildpack as asset to GitHub releases. This requires you have remote repo configured (named `origin`) and have specified API Key as parameter (or set via environmental variable).

## How to use

The buildpack can be either installed into the platform via `cf create-buildpack` option and reference in manifest by name, or (if allowed by platform settings) referenced via a URL.

The buildpacks created by this template are intended to be used in conjunction with HttpModuleLoaderBuildpack and HwcBuildpack. Module buildpacks such as the ones created by this template do not register themselves into the app - they simply supply the HttpModule and the necessary dependencies into the container. The registration of modules into the IIS pipeline is done by HttpModuleLoaderBuildpack which must be executed after all the module buildpacks have ran. HttpModuleLoaderBuildpack also ensures that there are no assembly version conflicts between multiple HttpModules and the app itself. It will take the necessary steps to ensure that multiple versions of assemblies are loaded side-by-side into the app domain. Ex. if your HttpModule uses JSON.NET v12 and the app uses JSON.NET v11, both assemblies will be loaded into the app domain and HttpModule and app will use version of the original version of the assembly they was compiled against. `hwc_buildpack` needs to be specified last in the manifest. 

#### Sample manifest


```yaml
applications:
- name: simpleapp
  stack: windows
  buildpacks: 
    - https://github.com/macsux/aspnet-logging-buildpack/releases/download/v0.1.0/AspNetLoggingBuildpack-win-x64-0.1.0.zip
    - https://github.com/macsux/http-module-loader-buildpack/releases/download/v0.1.0/HttpModuleLoaderBuildpack-win-x64-0.1.0.zip
    - hwc_buildpack
```

