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
[HarmonyPatch(typeof(QuestNode_GenerateThreats))]
internal static class Patch_QuestNode_GenerateThreats
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Factions;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(QuestNode_GenerateThreats.RunInt))]
    private static IEnumerable<CodeInstruction> RunInt_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("RunInt")
            .MatchCall(typeof(GenCollection), nameof(GenCollection.RandomElement), null, [typeof(Faction)])
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_QuestNode_GenerateThreats), nameof(RandomFactionWithFallback)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static Faction RandomFactionWithFallback(IEnumerable<Faction> source)
    {
        if (source.Any()) return source.RandomElement();
        return Find.FactionManager.RandomEnemyFaction();
    }
}
