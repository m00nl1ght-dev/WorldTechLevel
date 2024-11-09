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
        if (__result && WorldTechLevel.Settings.FilterPawnEquipment)
        {
            if (pair.thing.EffectiveTechLevel() > WorldTechLevel.Current)
            {
                WorldTechLevel.Logger.Log($"Filtered out apparel candidate {pair.thing.defName} for pawn {pawn.Name}");
                __result = false;
            }
        }
    }
}
