using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnApparelGenerator))]
internal static class Patch_PawnApparelGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Apparel;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnApparelGenerator.CanUsePair))]
    internal static void CanUsePair_Postfix(Pawn pawn, ThingStuffPair pair, ref bool __result)
    {
        if (__result && !pawn.IsStartingPawnGen())
        {
            if (pair.thing.EffectiveTechLevel() > WorldTechLevel.Current || pair.stuff?.EffectiveTechLevel() > WorldTechLevel.Current)
            {
                __result = false;
            }
        }
    }
}
