using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ComplexThreatWorker))]
internal static class Patch_ComplexThreatWorker
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(ComplexThreatWorker.CanResolve))]
    internal static void CanResolve_Postfix(ComplexThreatWorker __instance, ref bool __result)
    {
        if (__instance.def.MinRequiredTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
