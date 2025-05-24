using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ResearchProjectDef))]
internal static class Patch_ResearchProjectDef
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchProjectDef.CanStartNow), MethodType.Getter)]
    internal static void CanStartNow_Postfix(ResearchProjectDef __instance, ref bool __result)
    {
        __result = __result && __instance.MinRequiredTechLevel() <= TechLevelUtility.PlayerResearchFilterLevel();
    }
}
