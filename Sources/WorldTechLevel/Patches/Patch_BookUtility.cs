using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(BookUtility))]
internal static class Patch_BookUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Items;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BookUtility.GetBookDefs))]
    internal static void GetBookDefs_Postfix(ref IEnumerable<ThingDef> __result)
    {
        __result = __result.FilterByMinRequiredTechLevel();
    }
}
