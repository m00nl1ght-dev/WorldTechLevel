using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_BetterResearchTabs : ModCompat
{
    public override string TargetAssemblyName => "TowersBetterResearchTabs";
    public override string DisplayName => "Better Research Tabs";

    private static MethodInfo _targetMethod;

    protected override bool OnApply()
    {
        var targetType = FindType("TowersBetterResearchTabs.MainTabWindow_Research");
        _targetMethod = Require(AccessTools.Method(targetType, "get_VisibleResearchProjects"));
        return true;
    }

    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod() => _targetMethod;

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Research;

    [HarmonyPrefix]
    internal static void GetVisibleResearchProjects_Prefix(List<ResearchProjectDef> ___cachedVisibleResearchProjects, ref bool __state)
    {
        __state = ___cachedVisibleResearchProjects == null;
    }

    [HarmonyPostfix]
    internal static void GetVisibleResearchProjects_Postfix(List<ResearchProjectDef> ___cachedVisibleResearchProjects, ref bool __state)
    {
        if (__state && WorldTechLevel.Current != TechLevel.Archotech)
        {
            var filterLevel = TechLevelUtility.PlayerResearchFilterLevel();

            ___cachedVisibleResearchProjects.RemoveAll(
                def => def.EffectiveTechLevel() > filterLevel && !def.IsFinished
            );
        }
    }
}
