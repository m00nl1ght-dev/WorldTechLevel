using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(ThingSetMakerUtility))]
internal static class Patch_ThingSetMakerUtility
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(ThingSetMakerUtility.GetAllowedThingDefs))]
    internal static void GetAllowedThingDefs_Postfix(ref IEnumerable<ThingDef> __result)
    {
        __result = __result.FilterByEffectiveTechLevel();
    }
}
