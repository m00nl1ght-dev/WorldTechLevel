using System;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WorldTechLevel;

public class BuildingMaterialUtility
{
    public static ThingDef RandomAppropriateBuildingMaterialFor(Map map, ThingDef thingDef, TechLevel techLevel, Predicate<ThingDef> validator = null)
    {
        if (!thingDef.MadeFromStuff) return null;

        techLevel = techLevel.ClampToWorld();

        if (thingDef.stuffCategories.Contains(StuffCategoryDefOf.Woody))
        {
            var extraWoodChance = techLevel switch
            {
                <= TechLevel.Neolithic => 0.7f,
                TechLevel.Medieval => 0.3f,
                _ => 0f,
            };

            if (Rand.Chance(extraWoodChance) && (validator == null || validator(ThingDefOf.WoodLog)))
                return ThingDefOf.WoodLog;
        }

        bool CanUse(ThingDef stuff)
        {
            if (stuff.stuffProps is not { allowedInStuffGeneration: true } props || !props.CanMake(thingDef))
                return false;

            if (props.categories.Contains(StuffCategoryDefOf.Metallic) && techLevel < TechLevel.Industrial)
                return false;

            if (!IsResourcePlentifulInMap(map, stuff))
                return false;

            if (stuff.EffectiveTechLevel() > WorldTechLevel.Current)
                return false;

            if (stuff.BaseMarketValue / stuff.VolumePerUnit >= 5f)
                return false;

            return validator == null || validator(stuff);
        }

        return GenStuff.StuffDefs.Where(CanUse).RandomElementWithFallback();
    }

    public static TerrainDef RandomAppropriateBasicFloorFor(Map map, Faction faction, bool allowCarpet)
    {
        var techLevel = faction?.def.EffectiveTechLevel().ClampToWorld() ?? WorldTechLevel.Current;
        var isCarpet = allowCarpet && !techLevel.IsNeolithicOrWorse() && Rand.Chance(0.1f);

        if (faction is { ideos: not null } && BaseGenUtility.IdeoFloorTypes(faction, isCarpet).TryRandomElement(out var result))
            return result;

        if (isCarpet)
            return DefDatabase<TerrainDef>.AllDefs.FilterByEffectiveTechLevel(techLevel).Where(x => x.IsCarpet).RandomElement();

        if (Rand.Chance(0.5f) && !map.IsPocketMap)
        {
            var stoneFloor = BaseGenUtility.RegionalRockTerrainDef(map.Tile, false);

            if (stoneFloor != TerrainDefOf.Concrete && stoneFloor.EffectiveTechLevel() <= techLevel)
                return stoneFloor;
        }

        if (techLevel >= TechLevel.Industrial)
            Rand.Element(TerrainDefOf.MetalTile, TerrainDefOf.PavedTile);

        return TerrainDefOf.WoodPlankFloor;
    }

    public static bool IsAppropriateFloorMaterial(Map map, TerrainDef floor)
    {
        if (floor.EffectiveTechLevel() > WorldTechLevel.Current)
            return false;

        if (floor.costList?.Any(c => c.thingDef.IsStuff && !IsResourcePlentifulInMap(map, c.thingDef)) ?? false)
            return false;

        return true;
    }

    public static bool IsResourcePlentifulInMap(Map map, ThingDef stuff)
    {
        if (stuff.stuffProps.SourceNaturalRock is { IsNonResourceNaturalRock: true } && !map.IsPocketMap)
            return Find.World.NaturalRockTypesIn(map.Tile).Contains(stuff.stuffProps.SourceNaturalRock);

        if (stuff.defName == "DankPyon_DarkWood")
            return map.Biome.defName is "DankPyon_AncientForest" or "DankPyon_DarkForest";

        if (stuff.defName == "DankPyon_Bone")
            return false;

        return true;
    }
}
