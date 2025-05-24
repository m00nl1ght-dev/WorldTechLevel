using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(global::ResearchUtility))]
internal static class Patch_ResearchUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(global::ResearchUtility.ApplyPlayerStartingResearch))]
    private static IEnumerable<CodeInstruction> ApplyPlayerStartingResearch_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("ApplyPlayerStartingResearch")
            .MatchLoad(typeof(MemeDef), nameof(MemeDef.startingResearchProjects)).Keep()
            .InsertCall(typeof(Patch_ResearchUtility), nameof(FilteredResearch));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static List<ResearchProjectDef> FilteredResearch(List<ResearchProjectDef> input)
    {
        return input.Where(def => def.MinRequiredTechLevel() <= WorldTechLevel.Current).ToList();
    }
}
