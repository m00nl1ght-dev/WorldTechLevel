using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(HiddenItemsManager))]
internal static class Patch_HiddenItemsManager
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() =>
        WorldTechLevel.Settings.Filter_Items &&
        WorldTechLevel.Settings.Filter_Research &&
        WorldTechLevel.Settings.MarkThingDefsAsHidden;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(HiddenItemsManager.Hidden))]
    internal static void Hidden_Postfix(ThingDef def, ref bool __result)
    {
        __result = __result || def.EffectiveTechLevel() > TechLevelUtility.PlayerResearchFilterLevel();
    }
}
