using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(NaturalRandomQuestChooser))]
internal static class Patch_NaturalRandomQuestChooser
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod() =>
        AccessTools.FindIncludingInnerTypes(typeof(NaturalRandomQuestChooser), type => AccessTools.FirstMethod(type, method =>
            method.Name.Contains("<ChooseNaturalRandomQuest>") && method.ReturnType == typeof(bool)));

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ChooseNaturalRandomQuest_TryGetQuest_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("TryGetQuest")
            .MatchCall(typeof(DefDatabase<QuestScriptDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_NaturalRandomQuestChooser), nameof(FilteredQuests)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<QuestScriptDef> FilteredQuests()
    {
        return DefDatabase<QuestScriptDef>.AllDefs.FilterByMinRequiredTechLevel();
    }
}
