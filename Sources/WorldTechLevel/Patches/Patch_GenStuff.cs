using System;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(GenStuff))]
internal static class Patch_GenStuff
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_BuildingMaterials;

    [HarmonyPrefix]
    [HarmonyPatch( nameof(GenStuff.RandomStuffInexpensiveFor), [typeof(ThingDef), typeof(TechLevel), typeof(Predicate<ThingDef>)])]
    private static bool RandomStuffInexpensiveFor_Prefix(ThingDef thingDef, TechLevel maxTechLevel, Predicate<ThingDef> validator, ref ThingDef __result)
    {
        if (MapGenerator.mapBeingGenerated is not { } map || thingDef.building == null) return true;
        __result = TechLevelUtility.RandomAppropriateBuildingMaterialFor(map, thingDef, maxTechLevel, validator);
        return __result == null;
    }
}
