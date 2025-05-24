using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnTechHediffsGenerator))]
internal static class Patch_PawnTechHediffsGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Prosthetics;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnTechHediffsGenerator.InstallPart))]
    private static bool InstallPart_Prefix(Pawn pawn, ThingDef partDef)
    {
        return pawn.IsStartingPawnGen() || partDef.MinRequiredTechLevel() <= WorldTechLevel.Current;
    }
}
