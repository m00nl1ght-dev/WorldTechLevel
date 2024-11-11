using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(PawnAddictionHediffsGenerator))]
internal static class Patch_PawnAddictionHediffsGenerator
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnAddictionHediffsGenerator.PossibleWithTechLevel))]
    private static bool PossibleWithTechLevel_Prefix(ChemicalDef chemical, Faction faction)
    {
        var techLevel = faction?.def.techLevel.ClampToWorld() ?? WorldTechLevel.Current;

        return PawnAddictionHediffsGenerator.allDrugs
            .Any(x => x.GetCompProperties<CompProperties_Drug>().chemical == chemical && x.techLevel <= techLevel);
    }
}
