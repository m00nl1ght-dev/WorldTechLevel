using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class EffectiveTechLevels
{
    private static readonly List<TechLevel> _tmpList = [];

    internal static void Initialize()
    {
        TechLevelDatabase<ThingDef>.Initialize(ThingDefFirstPass);
        TechLevelDatabase<ThingDef>.ApplyOverrides();
        TechLevelDatabase<ThingDef>.Apply(ThingDefSecondPass);

        TechLevelDatabase<ResearchProjectDef>.Initialize(ResearchProjectDef);
        TechLevelDatabase<ResearchProjectDef>.ApplyOverrides();

        TechLevelDatabase<IdeoPresetDef>.Initialize();
        TechLevelDatabase<IdeoPresetDef>.ApplyOverrides();

        TechLevelDatabase<MemeDef>.Initialize();
        TechLevelDatabase<MemeDef>.ApplyOverrides();

        TechLevelDatabase<PreceptDef>.Initialize();
        TechLevelDatabase<PreceptDef>.ApplyOverrides();

        TechLevelDatabase<RitualAttachableOutcomeEffectDef>.Initialize();
        TechLevelDatabase<RitualAttachableOutcomeEffectDef>.ApplyOverrides();

        TechLevelDatabase<IncidentDef>.Initialize();
        TechLevelDatabase<IncidentDef>.ApplyOverrides();

        TechLevelDatabase<QuestScriptDef>.Initialize();
        TechLevelDatabase<QuestScriptDef>.ApplyOverrides();

        TechLevelDatabase<ComplexThreatDef>.Initialize();
        TechLevelDatabase<ComplexThreatDef>.ApplyOverrides();

        TechLevelDatabase<GenStepDef>.Initialize();
        TechLevelDatabase<GenStepDef>.ApplyOverrides();

        TechLevelDatabase<WorldGenStepDef>.Initialize();
        TechLevelDatabase<WorldGenStepDef>.ApplyOverrides();

        TechLevelDatabase<TraitDef>.Initialize();
        TechLevelDatabase<TraitDef>.ApplyOverrides();

        TechLevelDatabase<PawnKindDef>.Initialize(PawnKindDef);
        TechLevelDatabase<PawnKindDef>.ApplyOverrides();

        TechLevelDatabase<BackstoryDef>.Initialize(BackstoryDef);
        TechLevelDatabase<BackstoryDef>.ApplyOverrides();

        WarnPawnKindFactionUsages();
    }

    private static TechLevel ThingDefFirstPass(ThingDef def)
    {
        if (def is { generated: true, thingCategories: not null })
        {
            if (def.GetCompProperties<CompProperties_Techprint>() is { } techprint)
                return techprint.project.techLevel;

            if (def.thingCategories.Contains(ThingCategoryDefOf.NeurotrainersSkill))
                return WorldTechLevel.Settings.AlwaysAllowNeurotrainers ? TechLevel.Undefined : def.techLevel;

            if (def.thingCategories.Contains(ThingCategoryDefOf.NeurotrainersPsycast))
                return WorldTechLevel.Settings.AlwaysAllowNeurotrainers ? TechLevel.Undefined : def.techLevel;
        }

        if (def is { techLevel: TechLevel.Archotech, thingCategories: not null })
        {
            if (def.thingCategories.Contains(WTL_DefOf.InertRelics))
                return TechLevel.Undefined;
        }

        if (def.techLevel != TechLevel.Undefined)
            return def.techLevel;

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
        if (level != TechLevel.Undefined)
            return level;

        _tmpList.Clear();
        _tmpList.Add(TechLevel.Undefined);

        if (def.costList != null)
            foreach (var entry in def.costList)
                _tmpList.Add(entry.thingDef.EffectiveTechLevel());

        if (def.GetCompProperties<CompProperties_Power>() != null)
            _tmpList.Add(TechLevel.Industrial);

        if (def.GetCompProperties<CompProperties_Book>() != null)
            _tmpList.Add(TechLevel.Medieval);

        return _tmpList.Max();
    }

    private static TechLevel ResearchProjectDef(ResearchProjectDef def)
    {
        return def.techLevel;
    }

    private static TechLevel PawnKindDef(PawnKindDef def)
    {
        if (def.defaultFactionType is not { isPlayer: false })
            return TechLevel.Undefined;

        return def.defaultFactionType.techLevel;
    }

    private static TechLevel BackstoryDef(BackstoryDef def)
    {
        if (!def.shuffleable)
            return TechLevel.Undefined;

        if (def.spawnCategories?.Contains("Tribal") ?? false)
            return TechLevel.Neolithic;

        var title = def.untranslatedTitle?.ToLower() ?? "";
        var desc = def.untranslatedDesc?.ToLower() ?? "";

        var techLevel = TechLevel.Medieval;

        foreach (var config in DefDatabase<TechLevelConfigDef>.AllDefs)
        {
            if (config.storyFilters != null)
            {
                foreach (var filter in config.storyFilters)
                {
                    if (filter.techLevel > techLevel)
                    {
                        if (filter.strongTerms?.Any(term => title.Contains(term) || desc.Contains(term)) ?? false)
                            return filter.techLevel;

                        if (filter.weakTerms?.Any(term => title.Contains(term) || desc.Contains(term)) ?? false)
                            techLevel = filter.techLevel;
                    }
                }
            }
        }

        return techLevel;
    }

    private static void WarnPawnKindFactionUsages()
    {
        foreach (var faction in DefDatabase<FactionDef>.AllDefs.Where(d => d.pawnGroupMakers != null))
        {
            foreach (var kind in faction.pawnGroupMakers.SelectMany(g => g.options).Select(o => o.kind).Distinct())
            {
                if (kind.EffectiveTechLevel() > faction.techLevel && kind.GetAlternative(faction.techLevel) == null)
                {
                    WorldTechLevel.Logger.Warn(
                        $"Pawn kind {kind.defName} ({kind.EffectiveTechLevel()}) " +
                        $"is used by faction {faction.defName} with lower tech level ({faction.techLevel})"
                    );
                }
            }
        }
    }
}
