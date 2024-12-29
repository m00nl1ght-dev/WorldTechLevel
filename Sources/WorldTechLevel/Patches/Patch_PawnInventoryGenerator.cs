using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnInventoryGenerator))]
internal static class Patch_PawnInventoryGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Possessions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnInventoryGenerator.GenerateInventoryFor))]
    private static void GenerateInventoryFor_Postfix(Pawn p)
    {
        if (!p.IsStartingPawnGen())
        {
            p.inventory.innerContainer.RemoveAll(thing => thing.def.EffectiveTechLevel() > WorldTechLevel.Current);
        }
    }
}
