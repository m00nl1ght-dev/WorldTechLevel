using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ResearchUtility))]
internal static class Patch_ResearchUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ResearchUtility.ApplyPlayerStartingResearch))]
    private static IEnumerable<CodeInstruction> ApplyPlayerStartingResearch_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("ApplyPlayerStartingResearch")
            .MatchCall(typeof(DefDatabase<ResearchProjectDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_ResearchUtility), nameof(FilteredProjects)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<ResearchProjectDef> FilteredProjects()
    {
        return DefDatabase<ResearchProjectDef>.AllDefs.FilterByEffectiveTechLevel(TechLevelUtility.PlayerResearchFilterLevel());
    }
}
