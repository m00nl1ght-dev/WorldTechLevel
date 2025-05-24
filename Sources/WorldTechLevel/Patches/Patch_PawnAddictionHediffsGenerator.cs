using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnAddictionHediffsGenerator))]
internal static class Patch_PawnAddictionHediffsGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Addictions;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnAddictionHediffsGenerator.PossibleWithTechLevel))]
    private static bool PossibleWithTechLevel_Prefix(ChemicalDef chemical, Faction faction, ref bool __result)
    {
        if (Current.ProgramState == ProgramState.Entry && faction is { IsPlayer: true }) return true;

        __result = PawnAddictionHediffsGenerator.allDrugs
            .Any(x => x.GetCompProperties<CompProperties_Drug>().chemical == chemical && x.techLevel <= faction.CurrentFilterLevel());

        return false;
    }
}
