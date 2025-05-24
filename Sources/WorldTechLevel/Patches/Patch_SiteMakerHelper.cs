using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(SiteMakerHelper))]
internal static class Patch_SiteMakerHelper
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(SiteMakerHelper.SitePartDefsWithTag))]
    internal static void SitePartDefsWithTag_Postfix(ref IEnumerable<SitePartDef> __result)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            __result = __result.Where(p => p.MinRequiredTechLevel() <= WorldTechLevel.Current);
        }
    }
}
