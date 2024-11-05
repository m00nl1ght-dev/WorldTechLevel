using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_DubsMintMenus : ModCompat
{
    public override string TargetAssemblyName => "DubsMintMenus";
    public override string DisplayName => "Dubs Mint Menus";

    [HarmonyPrefix]
    [HarmonyPatch("DubsMintMenus.MainTabWindow_MintResearch", "parc")]
    private static bool Parc_Prefix(ResearchProjectDef p, ref bool __result)
    {
        if (p.EffectiveTechLevel() > WorldTechLevel.Current)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
