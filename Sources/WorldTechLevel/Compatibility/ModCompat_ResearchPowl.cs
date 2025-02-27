using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_ResearchPowl : ModCompat
{
    public override string TargetAssemblyName => "ResearchPowl";
    public override string DisplayName => "Research Powl";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyPostfix]
    [HarmonyPatch("ResearchPowl.ResearchNode", "GetAvailable")]
    private static void GetAvailable_Postfix(ResearchProjectDef ___Research, ref bool __result)
    {
        if (__result && ___Research.EffectiveTechLevel() > TechLevelUtility.PlayerResearchFilterLevel())
        {
            __result = false;
        }
    }
}
