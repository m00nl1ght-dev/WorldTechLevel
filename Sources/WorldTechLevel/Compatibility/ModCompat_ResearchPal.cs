using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_ResearchPal : ModCompat
{
    public override string TargetAssemblyName => "ResearchTree";
    public override string DisplayName => "Research Pal";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyPostfix]
    [HarmonyPatch("ResearchPal.CompatibilityHooks", "PassCustomUnlockRequirements")]
    private static void PassCustomUnlockRequirements_Postfix(ResearchProjectDef p, ref bool __result)
    {
        if (__result && p.EffectiveTechLevel() > TechLevelUtility.PlayerResearchFilterLevel())
        {
            __result = false;
        }
    }
}
