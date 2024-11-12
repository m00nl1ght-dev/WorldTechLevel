using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(PawnTechHediffsGenerator))]
internal static class Patch_PawnTechHediffsGenerator
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnTechHediffsGenerator.InstallPart))]
    private static bool InstallPart_Prefix(ThingDef partDef)
    {
        if (!WorldTechLevel.Settings.FilterPawnEquipment) return true;
        return partDef.EffectiveTechLevel() <= WorldTechLevel.Current;
    }
}
