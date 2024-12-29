using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WorldTechLevel;

public static class TechLevelUtility
{
    public static TechLevel Max(TechLevel a, TechLevel b) => a > b ? a : b;

    public static TechLevel ClampToWorld(this TechLevel techLevel)
    {
        return techLevel == TechLevel.Undefined || techLevel > WorldTechLevel.Current ? WorldTechLevel.Current : techLevel;
    }

    public static TechLevel GenFilterTechLevel(this Pawn pawn)
    {
        return pawn.IsStartingPawnGen() ? Max(pawn.Faction.def.techLevel, WorldTechLevel.Current) : WorldTechLevel.Current;
    }

    public static TechLevel EffectiveTechLevel<T>(this T def) where T : Def
    {
        TechLevelDatabase<T>.EnsureInitialized();

        var data = TechLevelDatabase<T>.Levels;
        if (def != null && def.index < data.Length)
            return data[def.index];

        return TechLevel.Undefined;
    }

    public static IEnumerable<T> FilterByEffectiveTechLevel<T>(this IEnumerable<T> defs, TechLevel techLevel) where T : Def
    {
        if (techLevel == TechLevel.Archotech)
            return defs;

        TechLevelDatabase<T>.EnsureInitialized();

        var data = TechLevelDatabase<T>.Levels;
        return defs.Where(def => def.index >= data.Length || data[def.index] <= techLevel);
    }

    public static IEnumerable<T> FilterByEffectiveTechLevel<T>(this IEnumerable<T> defs) where T : Def
    {
        return defs.FilterByEffectiveTechLevel(WorldTechLevel.Current);
    }

    public static T GetAlternative<T>(this T def) where T : Def => def.GetAlternative(WorldTechLevel.Current);

    public static T GetAlternative<T>(this T def, TechLevel targetLevel) where T : Def
    {
        TechLevelDatabase<T>.EnsureInitialized();

        var data = TechLevelDatabase<T>.Alternatives;
        if (def.index >= data.Length) return null;

        var alternatives = data[def.index];
        if (alternatives == null) return null;

        bool Filter(TechLevelDatabase<T>.Alternative option)
        {
            var level = option.def.EffectiveTechLevel();
            return level == targetLevel || level == TechLevel.Undefined;
        }

        while (!alternatives.Any(Filter))
        {
            if (targetLevel == TechLevel.Neolithic) return null;
            targetLevel -= 1;
        }

        return alternatives.Where(Filter).RandomElementByWeight(a => a.weight).def;
    }

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
        var techLevel = faction?.def.techLevel.ClampToWorld() ?? WorldTechLevel.Current;
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

    public static bool IsStartingPawnGen(this Pawn pawn)
    {
        return Current.ProgramState == ProgramState.Entry && pawn.Faction is { IsPlayer: true };
    }

    public static TechLevel PlayerResearchFilterLevel()
    {
        return Max(Faction.OfPlayer.def.techLevel, WorldTechLevel.Current);
    }
}
