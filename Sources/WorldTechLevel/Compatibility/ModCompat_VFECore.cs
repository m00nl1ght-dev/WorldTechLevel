using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_VFECore : ModCompat
{
    public override string TargetAssemblyName => "VFECore";
    public override string DisplayName => "Vanilla Expanded Framework";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_GenSteps;

    [HarmonyPostfix]
    [HarmonyPatch("VFECore.MapGenerator_GenerateMap_Patch", "GetThingDefToSpawn")]
    private static void GetThingDefToSpawn_Postfix(ref ThingDef __result)
    {
        if (__result != null && __result.MinRequiredTechLevel() > WorldTechLevel.Current)
        {
            __result = null;
        }
    }
}
