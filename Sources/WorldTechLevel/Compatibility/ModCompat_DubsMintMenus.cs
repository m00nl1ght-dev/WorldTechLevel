using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_DubsMintMenus : ModCompat
{
    public override string TargetAssemblyName => "DubsMintMenus";
    public override string DisplayName => "Dubs Mint Menus";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyPrefix]
    [HarmonyPatch("DubsMintMenus.MainTabWindow_MintResearch", "parc")]
    private static bool Parc_Prefix(ResearchProjectDef p, ref bool __result)
    {
        if (p.MinRequiredTechLevel() > TechLevelUtility.PlayerResearchFilterLevel())
        {
            __result = false;
            return false;
        }

        return true;
    }
}
