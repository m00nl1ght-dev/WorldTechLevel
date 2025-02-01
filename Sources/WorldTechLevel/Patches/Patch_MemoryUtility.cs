using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse.Profile;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(MemoryUtility))]
internal static class Patch_MemoryUtility
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MemoryUtility.ClearAllMapsAndWorld))]
    internal static void ClearAllMapsAndWorld_Postfix()
    {
        WorldTechLevel.Current = TechLevel.Archotech;
        ResearchUtility.InitialResearchLevel = TechLevel.Undefined;
        ResearchUtility.CurrentResearchLevel = TechLevel.Undefined;
    }
}
