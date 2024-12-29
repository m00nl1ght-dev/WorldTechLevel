using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PreceptWorker))]
internal static class Patch_PreceptWorker
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Ideoligions;

    [HarmonyTargetMethods]
    private static IEnumerable<MethodInfo> TargetMethods() =>
        typeof(PreceptWorker).AllSubclasses().Prepend(typeof(PreceptWorker))
            .Select(t => AccessTools.PropertyGetter(t, nameof(PreceptWorker.ThingDefs)))
            .Where(m => m != null);

    [HarmonyPostfix]
    internal static void GetThingDefs_Postfix(ref IEnumerable<PreceptThingChance> __result)
    {
        __result = __result.Where(t => t.def.EffectiveTechLevel() <= WorldTechLevel.Current);
    }
}
