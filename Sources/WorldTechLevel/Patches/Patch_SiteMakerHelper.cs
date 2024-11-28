using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(SiteMakerHelper))]
internal static class Patch_SiteMakerHelper
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(SiteMakerHelper.SitePartDefsWithTag))]
    internal static void SitePartDefsWithTag_Postfix(ref IEnumerable<SitePartDef> __result)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            __result = __result.Where(p => p.EffectiveTechLevel() <= WorldTechLevel.Current);
        }
    }
}
