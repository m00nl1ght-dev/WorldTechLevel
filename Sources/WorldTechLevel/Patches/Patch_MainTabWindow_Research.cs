using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(MainTabWindow_Research))]
internal static class Patch_MainTabWindow_Research
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(MainTabWindow_Research.VisibleResearchProjects), MethodType.Getter)]
    internal static void GetVisibleResearchProjects_Prefix(MainTabWindow_Research __instance, ref bool __state)
    {
        __state = __instance.cachedVisibleResearchProjects == null;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(MainTabWindow_Research.VisibleResearchProjects), MethodType.Getter)]
    internal static void GetVisibleResearchProjects_Postfix(MainTabWindow_Research __instance, ref bool __state)
    {
        if (__state && WorldTechLevel.Current != TechLevel.Archotech)
        {
            __instance.cachedVisibleResearchProjects.RemoveAll(
                def => def.EffectiveTechLevel() > WorldTechLevel.Current && !def.IsFinished
            );
        }
    }
}
