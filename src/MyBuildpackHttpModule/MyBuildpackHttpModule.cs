using System.Web;
using HarmonyLib;

namespace MyBuildpackHttpModule;

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