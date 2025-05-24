using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(BaseGenUtility))]
internal static class Patch_BaseGenUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_BuildingMaterials;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BaseGenUtility.RandomCheapWallStuff), [typeof(TechLevel), typeof(bool)])]
    private static bool RandomCheapWallStuff_Prefix(TechLevel techLevel, bool notVeryFlammable, ref ThingDef __result)
    {
        if (MapGenerator.mapBeingGenerated is not { } map) return true;
        Predicate<ThingDef> validator = notVeryFlammable ? def => def.BaseFlammability < 0.5 : null;
        __result = BuildingMaterialUtility.RandomAppropriateBuildingMaterialFor(map, ThingDefOf.Wall, techLevel, validator);
        return __result == null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BaseGenUtility.CheapStuffFor))]
    private static bool CheapStuffFor_Prefix(ThingDef thingDef, Faction faction, ref ThingDef __result)
    {
        if (MapGenerator.mapBeingGenerated is not { } map) return true;
        var techLevel = faction?.def.MinRequiredTechLevel() ?? TechLevel.Undefined;
        __result = BuildingMaterialUtility.RandomAppropriateBuildingMaterialFor(map, thingDef, techLevel);
        return __result == null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BaseGenUtility.RandomBasicFloorDef))]
    private static bool RandomBasicFloorDef_Prefix(Faction faction, bool allowCarpet, ref TerrainDef __result)
    {
        if (MapGenerator.mapBeingGenerated is not { } map) return true;
        __result = BuildingMaterialUtility.RandomAppropriateBasicFloorFor(map, faction, allowCarpet);
        return __result == null;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BaseGenUtility.TryRandomInexpensiveFloor))]
    private static IEnumerable<CodeInstruction> TryRandomInexpensiveFloor_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("TryRandomInexpensiveFloor")
            .MatchCall(typeof(DefDatabase<TerrainDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_BaseGenUtility), nameof(FilteredFloors)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<TerrainDef> FilteredFloors()
    {
        var map = MapGenerator.mapBeingGenerated;
        if (map == null) return DefDatabase<TerrainDef>.AllDefs;
        return DefDatabase<TerrainDef>.AllDefs.Where(def => BuildingMaterialUtility.IsAppropriateFloorMaterial(map, def));
    }
}
