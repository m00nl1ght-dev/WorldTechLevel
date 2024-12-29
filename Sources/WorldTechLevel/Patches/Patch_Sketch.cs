using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(Sketch))]
internal static class Patch_Sketch
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_BuildingMaterials;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Sketch.AddTerrain))]
    private static void AddTerrain_Prefix(ref TerrainDef def)
    {
        if (MapGenerator.mapBeingGenerated is { IsPocketMap: false } map)
        {
            if (!TechLevelUtility.IsAppropriateFloorMaterial(map, def))
            {
                Rand.PushState(def.index);
                def = BaseGenUtility.RegionalRockTerrainDef(map.Tile, false);
                Rand.PopState();
            }
        }
    }
}
