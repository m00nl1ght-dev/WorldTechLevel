using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(Root_Play))]
internal static class Patch_Root_Play
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Root_Play.SetupForQuickTestPlay))]
    private static void SetupForQuickTestPlay_Prefix()
    {
        WorldTechLevel.Current = WorldTechLevel.Settings.DevQuickTestWorldTechLevel;
    }
}
