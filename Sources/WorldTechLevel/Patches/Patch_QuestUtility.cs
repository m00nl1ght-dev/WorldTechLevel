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
        return quest?.root == null || quest.root.MinRequiredTechLevel() <= WorldTechLevel.Current;
    }
}
