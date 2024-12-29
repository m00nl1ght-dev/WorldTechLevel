using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ThingSetMakerUtility))]
internal static class Patch_ThingSetMakerUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Items;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(ThingSetMakerUtility.GetAllowedThingDefs))]
    internal static void GetAllowedThingDefs_Postfix(ref IEnumerable<ThingDef> __result)
    {
        __result = __result.FilterByEffectiveTechLevel();
    }
}
