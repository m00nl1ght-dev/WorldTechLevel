using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(ResearchManager))]
internal static class Patch_ResearchManager
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchManager.FinishProject))]
    internal static void FinishProject_Postfix()
    {
        ResearchUtility.RefreshCurrentResearchLevel();
    }
}
