using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(MapGenerator))]
internal static class Patch_MapGenerator
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(MapGenerator.GenerateContentsIntoMap))]
    private static void GenerateContentsIntoMap_Prefix(ref IEnumerable<GenStepWithParams> genStepDefs)
    {
        genStepDefs = genStepDefs.Where(f => f.def.EffectiveTechLevel() <= WorldTechLevel.Current);
    }
}
