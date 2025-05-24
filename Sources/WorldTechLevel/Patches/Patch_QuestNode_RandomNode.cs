using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.QuestGen;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(QuestNode_RandomNode))]
internal static class Patch_QuestNode_RandomNode
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(QuestNode_RandomNode.GetNodesCanRun))]
    internal static void GetNodesCanRun_Postfix(Slate slate, ref IEnumerable<QuestNode> __result)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            __result = __result.Filter(slate);
        }
    }

    private static IEnumerable<QuestNode> Filter(this IEnumerable<QuestNode> nodes, Slate slate)
    {
        foreach (var node in nodes)
        {
            if (node is QuestNode_SubScript subscript && subscript.def.TryGetValue(slate, out var def))
            {
                if (def.MinRequiredTechLevel() > WorldTechLevel.Current) continue;
            }

            yield return node;
        }
    }
}
