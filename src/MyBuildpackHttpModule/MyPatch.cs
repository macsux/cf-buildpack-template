using HarmonyLib;

namespace MyBuildpackHttpModule;


/// <summary>
/// Example on how to write a "monkey-patch" that rewrites DateTime.ToString() method to return something else
/// </summary>
[HarmonyPatch(typeof(DateTime), nameof(ToString), new Type[0])]
public class MyPatch
{
    static void Postfix(ref string __result)
    {
        __result = "Highjacked!";
    }
}