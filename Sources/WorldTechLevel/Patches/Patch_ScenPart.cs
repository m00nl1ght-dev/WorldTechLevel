using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(ScenPart))]
internal static class Patch_ScenPart
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ScenPart_StartingThing_Defined), nameof(ScenPart.PlayerStartingThings))]
    internal static void StartingThing_Defined_PlayerStartingThings_Postfix(ref IEnumerable<Thing> __result)
    {
        if (WorldTechLevel.Settings.FilterStartingPossessions)
        {
            __result = __result.Where(t => t.def.EffectiveTechLevel() <= WorldTechLevel.Current);
        }
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ScenPart_ScatterThings), nameof(ScenPart.GenerateIntoMap))]
    internal static bool ScatterThings_GenerateIntoMap_Prefix(ScenPart_ScatterThings __instance)
    {
        if (!WorldTechLevel.Settings.FilterStartingPossessions) return true;
        return __instance.thingDef.EffectiveTechLevel() <= WorldTechLevel.Current;
    }
}
