using System.Web;
using HarmonyLib;
using JetBrains.Annotations;

namespace MyBuildpackHttpModule;

[PublicAPI]
public class MyBuildpackHttpModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        var harmony = new Harmony("MyBuildpack");
        harmony.PatchAll();
    }
       
    public void Dispose()
    {
    }
}