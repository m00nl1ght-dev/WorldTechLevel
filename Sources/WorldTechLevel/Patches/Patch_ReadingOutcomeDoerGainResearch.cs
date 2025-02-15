using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ReadingOutcomeDoerGainResearch))]
internal static class Patch_ReadingOutcomeDoerGainResearch
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Items;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ReadingOutcomeDoerGainResearch.IsValid))]
    internal static void IsValid_Postfix(ResearchProjectDef project, ref bool __result)
    {
        if (__result && project.EffectiveTechLevel() > WorldTechLevel.Current)
        {
            __result = false;
        }
    }
}
