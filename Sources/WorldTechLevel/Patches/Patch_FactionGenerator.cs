using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(FactionGenerator))]
internal static class Patch_FactionGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Factions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(FactionGenerator.ConfigurableFactions), MethodType.Getter)]
    internal static void GetConfigurableFactions(ref IEnumerable<FactionDef> __result)
    {
        __result = __result.Where(f => f.techLevel <= WorldTechLevel.Current);
    }
}
