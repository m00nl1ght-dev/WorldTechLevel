using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class TechLevelUtility
{
    public static TechLevel EffectiveTechLevel<T>(this T def) where T : Def
    {
        var data = TechLevelDatabase<T>.Levels;
        if (def.index < data.Length) return data[def.index];
        return TechLevel.Undefined;
    }

    public static IEnumerable<T> FilterByEffectiveTechLevel<T>(this IEnumerable<T> defs, TechLevel techLevel) where T : Def
    {
        if (techLevel == TechLevel.Archotech) return defs;
        var data = TechLevelDatabase<T>.Levels;
        return defs.Where(def => def.index >= data.Length || data[def.index] <= techLevel);
    }

    public static IEnumerable<T> FilterByEffectiveTechLevel<T>(this IEnumerable<T> defs) where T : Def
    {
        return defs.FilterByEffectiveTechLevel(WorldTechLevel.Current);
    }

    public static T GetAlternative<T>(this T def) where T : Def
    {
        var data = TechLevelDatabase<T>.Alternatives;
        if (def.index >= data.Length) return null;

        var alternatives = data[def.index];
        if (alternatives == null) return null;

        var targetLevel = WorldTechLevel.Current;

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
}
