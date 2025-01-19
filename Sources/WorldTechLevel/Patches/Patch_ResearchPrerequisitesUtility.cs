using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ResearchPrerequisitesUtility))]
internal static class Patch_ResearchPrerequisitesUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchPrerequisitesUtility.UnlockedDefsGroupedByPrerequisites))]
    internal static void UnlockedDefsGroupedByPrerequisites_Postfix(ref List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> __result)
    {
        var filterLevel = TechLevelUtility.PlayerResearchFilterLevel();
        __result.RemoveAll(p => p.first.unlockedBy.Any(r => r.EffectiveTechLevel() > filterLevel));
    }
}
