using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_VFECore : ModCompat
{
    #if RW_1_6_OR_GREATER
    public override string TargetAssemblyName => "VEF";
    #else
    public override string TargetAssemblyName => "VFECore";
    #endif

    public override string DisplayName => "Vanilla Expanded Framework";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_GenSteps;

    [HarmonyPostfix]
    #if RW_1_6_OR_GREATER
    [HarmonyPatch("VEF.Maps.VanillaExpandedFramework_MapGenerator_GenerateMap_Patch", "GetThingDefToSpawn")]
    #else
    [HarmonyPatch("VFECore.MapGenerator_GenerateMap_Patch", "GetThingDefToSpawn")]
    #endif
    private static void GetThingDefToSpawn_Postfix(ref ThingDef __result)
    {
        if (__result != null && __result.MinRequiredTechLevel() > WorldTechLevel.Current)
        {
            __result = null;
        }
    }
}
