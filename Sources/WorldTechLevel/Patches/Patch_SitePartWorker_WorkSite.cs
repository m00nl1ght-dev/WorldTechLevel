using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(SitePartWorker_WorkSite))]
internal static class Patch_SitePartWorker_WorkSite
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_MineableResources;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SitePartWorker_WorkSite.LootThings))]
    internal static void LootThings_Postfix(ref IEnumerable<SitePartWorker_WorkSite.CampLootThingStruct> __result)
    {
        __result = __result.Where(e => e.thing.EffectiveTechLevel() <= WorldTechLevel.Current);
    }
}
