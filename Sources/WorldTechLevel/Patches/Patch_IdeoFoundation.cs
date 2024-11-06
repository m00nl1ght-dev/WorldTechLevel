using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(IdeoFoundation))]
internal static class Patch_IdeoFoundation
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(IdeoFoundation.CanAdd))]
    internal static void CanAdd_Postfix(PreceptDef precept, ref AcceptanceReport __result)
    {
        if (precept.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
