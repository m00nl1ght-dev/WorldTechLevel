using System.Collections.Generic;
using System.Linq;
using RimWorld;
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

    public static TechLevel PlayerResearchFilterLevel()
    {
        return Max(Faction.OfPlayer.def.techLevel, WorldTechLevel.Current);
    }

    public static TechLevel EffectiveTechLevel<T>(this T def) where T : Def
    {
        TechLevelDatabase<T>.EnsureInitialized();

        var data = TechLevelDatabase<T>.Levels;
        if (def != null && def.index < data.Length)
            return data[def.index];

        return TechLevel.Undefined;
    }

    public static TechLevel EffectiveTechLevel(this Thing thing)
    {
        return Max(thing.def.EffectiveTechLevel(), thing.Stuff.EffectiveTechLevel());
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
}
