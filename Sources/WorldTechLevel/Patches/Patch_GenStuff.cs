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
        __result = BuildingMaterialUtility.RandomAppropriateBuildingMaterialFor(map, thingDef, maxTechLevel.ClampTo(WorldTechLevel.Current), validator);
        return __result == null;
    }

    [HarmonyPrefix]
    [HarmonyPatch( nameof(GenStuff.RandomStuffInexpensiveFor), [typeof(ThingDef), typeof(Faction), typeof(Predicate<ThingDef>)])]
    private static bool RandomStuffInexpensiveFor_Prefix(ThingDef thingDef, Faction faction, Predicate<ThingDef> validator, ref ThingDef __result)
    {
        if (MapGenerator.mapBeingGenerated is not { } map || thingDef.building == null) return true;
        var techLevel = faction == null ? WorldTechLevel.Current : faction.def.TechLevelClamped();
        __result = BuildingMaterialUtility.RandomAppropriateBuildingMaterialFor(map, thingDef, techLevel, validator);
        return __result == null;
    }
}
