using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_ResearchTree : ModCompat
{
    public override string TargetAssemblyName => "FluffyResearchTree";
    public override string DisplayName => "Anomaly Supported Research Tree (Continued)";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research && ModsConfig.IsActive("fluffy.researchtree");

    [HarmonyPrefix]
    [HarmonyPatch("FluffyResearchTree.ResearchNode", "IsVisible")]
    private static bool IsVisible_Prefix(ResearchProjectDef ___Research)
    {
        return ___Research.EffectiveTechLevel() <= TechLevelUtility.PlayerResearchFilterLevel();
    }
}
