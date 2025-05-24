using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(QuestNode_Root_WorkSite))]
internal static class Patch_QuestNode_Root_WorkSite
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Factions;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(QuestNode_Root_WorkSite.GenerateSite))]
    private static IEnumerable<CodeInstruction> GenerateSite_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("GenerateSite")
            .MatchCall(typeof(DefDatabase<FactionDef>), "get_AllDefsListForReading")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_QuestNode_Root_WorkSite), nameof(FilteredDefs)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<FactionDef> FilteredDefs()
    {
        return DefDatabase<FactionDef>.AllDefs.Where(f => f.MinRequiredTechLevel() <= WorldTechLevel.Current);
    }
}
