using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(PawnWeaponGenerator))]
internal static class Patch_PawnWeaponGenerator
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnWeaponGenerator.GetCommonality))]
    internal static void GetCommonality_Postfix(Pawn pawn, ThingStuffPair pair, ref float __result)
    {
        if (__result > 0f && WorldTechLevel.Settings.FilterPawnEquipment)
        {
            if (pair.thing.EffectiveTechLevel() > WorldTechLevel.Current || pair.stuff.EffectiveTechLevel() > WorldTechLevel.Current)
            {
                // WorldTechLevel.Logger.Log($"Filtered out weapon candidate {pair.thing} ({pair.stuff}) for pawn {pawn}");
                __result = 0f;
            }
        }
    }
}
