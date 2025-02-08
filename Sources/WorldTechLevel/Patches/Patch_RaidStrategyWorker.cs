using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(RaidStrategyWorker))]
internal static class Patch_RaidStrategyWorker
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Incidents;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(RaidStrategyWorker.CanUseWith))]
    internal static void CanUseWith_Postfix(RaidStrategyWorker __instance, IncidentParms parms, ref bool __result)
    {
        if (__result && __instance.def.EffectiveTechLevel() > WorldTechLevel.Current)
        {
            __result = parms.faction != null && parms.faction.def.EffectiveTechLevel() <= WorldTechLevel.Current;
        }
    }
}
