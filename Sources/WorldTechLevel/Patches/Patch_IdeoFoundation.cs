using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(IdeoFoundation))]
internal static class Patch_IdeoFoundation
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Ideoligions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(IdeoFoundation.CanAdd))]
    internal static void CanAdd_Postfix(PreceptDef precept, ref AcceptanceReport __result)
    {
        if (precept.MinRequiredTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
