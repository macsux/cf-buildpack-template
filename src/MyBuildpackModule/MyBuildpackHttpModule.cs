using System.Web;
using HarmonyLib;

namespace MyBuildpackModule
{
    public class MyBuildpackHttpModule: IHttpModule
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
}