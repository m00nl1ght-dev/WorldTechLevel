using System;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(GenStuff))]
internal static class Patch_GenStuff
{
    [HarmonyPrefix]
    [HarmonyPatch( nameof(GenStuff.RandomStuffInexpensiveFor), [typeof(ThingDef), typeof(TechLevel), typeof(Predicate<ThingDef>)])]
    private static bool RandomStuffInexpensiveFor_Prefix(ThingDef thingDef, TechLevel maxTechLevel, Predicate<ThingDef> validator, ref ThingDef __result)
    {
        if (MapGenerator.mapBeingGenerated is not { } map || thingDef.building == null || !WorldTechLevel.Settings.FilterBuildingMaterials) return true;
        __result = TechLevelUtility.RandomAppropriateBuildingMaterialFor(map, thingDef, maxTechLevel, validator);
        return __result == null;
    }
}
