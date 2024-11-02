using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(FactionDef))]
internal static class Patch_FactionDef
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FactionDef.Description), MethodType.Getter)]
    internal static void GetDescription_Postfix(FactionDef __instance, ref string __result)
    {
        if (__instance.techLevel != TechLevel.Undefined)
        {
            __result += $"\n\n{"WorldTechLevel.TechLevel".Translate().CapitalizeFirst()}: ".AsTipTitle();
            __result += __instance.techLevel.ToStringHuman().CapitalizeFirst();
        }
    }
}
