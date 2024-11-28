using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(PawnApparelGenerator))]
internal static class Patch_PawnApparelGenerator
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnApparelGenerator.CanUsePair))]
    internal static void CanUsePair_Postfix(Pawn pawn, ThingStuffPair pair, ref bool __result)
    {
        if (__result && TechLevelUtility.ShouldFilterEquipmentFor(pawn))
        {
            if (pair.thing.EffectiveTechLevel() > WorldTechLevel.Current || pair.stuff?.EffectiveTechLevel() > WorldTechLevel.Current)
            {
                __result = false;
            }
        }
    }
}
