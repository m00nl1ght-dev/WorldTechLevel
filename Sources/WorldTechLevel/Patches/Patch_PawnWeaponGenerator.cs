using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnWeaponGenerator))]
internal static class Patch_PawnWeaponGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Weapons;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnWeaponGenerator.GetCommonality))]
    internal static void GetCommonality_Postfix(Pawn pawn, ThingStuffPair pair, ref float __result)
    {
        if (__result > 0f && !pawn.IsStartingPawnGen())
        {
            if (pair.thing.EffectiveTechLevel() > WorldTechLevel.Current || pair.stuff.EffectiveTechLevel() > WorldTechLevel.Current)
            {
                __result = 0f;
            }
        }
    }
}
