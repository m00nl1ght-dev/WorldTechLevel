
#if RW_1_6_OR_GREATER

using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(LandmarkDef))]
internal static class Patch_LandmarkDef
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_WorldGenSteps;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(LandmarkDef.EverValid))]
    internal static void EverValid_Postfix(LandmarkDef __instance, ref bool __result)
    {
        if (__result && __instance.MinRequiredTechLevel() > WorldTechLevel.Current) __result = false;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(LandmarkDef.IsValidTile))]
    internal static void IsValidTile_Postfix(LandmarkDef __instance, ref bool __result)
    {
        if (__result && __instance.MinRequiredTechLevel() > WorldTechLevel.Current) __result = false;
    }
}

#endif
