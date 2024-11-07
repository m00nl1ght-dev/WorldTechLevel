using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(ComplexThreatWorker))]
internal static class Patch_ComplexThreatWorker
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(ComplexThreatWorker.CanResolve))]
    internal static void CanResolve_Postfix(ComplexThreatWorker __instance, ref bool __result)
    {
        if (__instance.def.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}