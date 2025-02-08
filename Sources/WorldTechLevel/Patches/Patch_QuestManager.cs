using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(QuestManager))]
internal static class Patch_QuestManager
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(QuestManager.Add))]
    internal static bool Add_Prefix(Quest quest)
    {
        return quest?.root == null || quest.root.EffectiveTechLevel() <= WorldTechLevel.Current;
    }
}
