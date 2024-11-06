using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(TechprintUtility))]
internal static class Patch_TechprintUtility
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(TechprintUtility.GetResearchProjectsNeedingTechprintsNow))]
    internal static void GetResearchProjectsNeedingTechprintsNow_Postfix(ref IEnumerable<ResearchProjectDef> __result)
    {
        __result = __result.FilterByEffectiveTechLevel();
    }
}
