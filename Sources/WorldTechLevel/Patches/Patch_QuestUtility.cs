using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(QuestUtility))]
internal static class Patch_QuestUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(QuestUtility.SendLetterQuestAvailable))]
    internal static bool SendLetterQuestAvailable_Prefix(Quest quest)
    {
        if (quest?.root != null && quest.root.MinRequiredTechLevel() > WorldTechLevel.Current)
        {
            if (QuestRetrySafeguard.Check(quest.root, false))
            {
                return false;
            }
        }

        return true;
    }

    #if RW_1_6_OR_GREATER

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(QuestUtility.GetGiverQuests))]
    internal static void GetGiverQuests_Postfix(ref IEnumerable<QuestScriptDef> __result)
    {
        __result = __result.FilterByMinRequiredTechLevel();
    }

    #endif
}
