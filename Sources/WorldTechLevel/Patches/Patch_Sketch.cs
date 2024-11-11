using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(Sketch))]
internal static class Patch_Sketch
{
    [HarmonyPrefix]
    [HarmonyPatch( nameof(Sketch.AddTerrain))]
    private static void AddTerrain_Prefix(ref TerrainDef def)
    {
        if (WorldTechLevel.Settings.FilterBuildingMaterials)
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
}
