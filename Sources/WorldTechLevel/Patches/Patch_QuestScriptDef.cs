using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.QuestGen;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(QuestScriptDef))]
internal static class Patch_QuestScriptDef
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(QuestScriptDef.CanRun), [typeof(Slate), typeof(IIncidentTarget)])]
    internal static bool CanRun_Prefix(QuestScriptDef __instance, ref bool __result)
    {
        if (__instance.MinRequiredTechLevel() > WorldTechLevel.Current)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
