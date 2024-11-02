using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class EffectiveTechLevels
{
    private static readonly List<TechLevel> _tmpList = [];

    public static TechLevel EffectiveTechLevel<T>(this T def) where T : Def
    {
        var data = TechLevelDatabase<ThingDef>.Data;
        if (def.index < data.Length) return data[def.index];
        return TechLevel.Undefined;
    }

    internal static void Initialize()
    {
        TechLevelDatabase<ThingDef>.Initialize(ThingDefFirstPass);
        TechLevelDatabase<ThingDef>.ApplyOverrides();
        TechLevelDatabase<ThingDef>.Apply(ThingDefSecondPass);
        TechLevelDatabase<ThingDef>.DebugOutput();
    }

    private static TechLevel ThingDefFirstPass(ThingDef def)
    {
        if (def.techLevel != TechLevel.Undefined) return def.techLevel;

        if (def.generated)
        {
            var techprint = def.GetCompProperties<CompProperties_Techprint>();
            if (techprint != null) return techprint.project.techLevel;
        }

        _tmpList.Clear();
        _tmpList.Add(TechLevel.Undefined);

        if (def.researchPrerequisites != null)
            foreach (var project in def.researchPrerequisites)
                _tmpList.Add(project.techLevel);

        if (def.recipeMaker != null)
        {
            if (def.recipeMaker.researchPrerequisite != null)
                _tmpList.Add(def.recipeMaker.researchPrerequisite.techLevel);

            if (def.recipeMaker.researchPrerequisites != null)
                foreach (var project in def.recipeMaker.researchPrerequisites)
                    _tmpList.Add(project.techLevel);
        }

        return _tmpList.Max();
    }

    private static TechLevel ThingDefSecondPass(ThingDef def, TechLevel level)
    {
        if (level != TechLevel.Undefined) return level;

        _tmpList.Clear();
        _tmpList.Add(TechLevel.Undefined);

        if (def.costList != null)
            foreach (var entry in def.costList)
                _tmpList.Add(entry.thingDef.EffectiveTechLevel());

        return _tmpList.Max();
    }
}
