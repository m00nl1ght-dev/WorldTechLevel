using System.Collections.Generic;
using System.Linq;
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
    [HarmonyPatch(nameof(ThingSetMakerUtility.GetAllowedThingDefs))]
    internal static void GetAllowedThingDefs_Postfix(ref IEnumerable<ThingDef> __result)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            __result = __result.Where(t => t.EffectiveTechLevel() <= WorldTechLevel.Current);
        }
    }
}
