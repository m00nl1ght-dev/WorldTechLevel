using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(PawnInventoryGenerator))]
internal static class Patch_PawnInventoryGenerator
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnInventoryGenerator.GenerateInventoryFor))]
    private static void GenerateInventoryFor_Postfix(Pawn p)
    {
        if (TechLevelUtility.ShouldFilterEquipmentFor(p))
        {
            p.inventory.innerContainer.RemoveAll(thing =>
            {
                if (thing.def.EffectiveTechLevel() <= WorldTechLevel.Current) return false;
                WorldTechLevel.Logger.Log($"Filtered out inventory item {thing.def} for pawn {p}");
                return true;
            });
        }
    }
}
