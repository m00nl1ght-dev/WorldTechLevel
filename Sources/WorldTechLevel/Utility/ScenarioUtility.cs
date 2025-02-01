using System.Linq;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class ScenarioUtility
{
    public static TechLevel InherentResearchLevel = TechLevel.Undefined;

    public static void InitializeFor(Scenario scenario)
    {
        InherentResearchLevel = InherentResearchLevelFor(scenario);
    }

    /// <summary>
    /// Determine the research tech level that the player faction from the given scenario
    /// should have access to at minimum, regardless of world tech level.
    /// </summary>
    public static TechLevel InherentResearchLevelFor(Scenario scenario)
    {
        if (scenario == null)
        {
            WorldTechLevel.Logger.Error("No active scenario, could not determine player faction tech level");
            return TechLevel.Undefined;
        }

        // tech level from faction def defined by its author
        var techLevel = scenario.playerFaction.factionDef.techLevel;

        // check starting research from player faction
        if (scenario.playerFaction.factionDef.startingResearchTags is { } startingResearchTags)
            foreach (var startingResearchTag in startingResearchTags)
                foreach (var project in DefDatabase<ResearchProjectDef>.AllDefs)
                    if (project.HasTag(startingResearchTag))
                        techLevel = TechLevelUtility.Max(techLevel, project.EffectiveTechLevel());

        // check scenario parts
        foreach (var part in scenario.parts)
        {
            if (part is ScenPart_StartingResearch { project: not null } startingResearch)
            {
                // starting research from scenario may imply higher tech level
                techLevel = TechLevelUtility.Max(techLevel, startingResearch.project.EffectiveTechLevel());
            }
            else if (part is ScenPart_ConfigPage_ConfigureStartingPawns_KindDefs configPage)
            {
                if (Enumerable.Any(configPage.kindCounts, k => IsMechanitorPawnKind(k.kindDef)))
                {
                    // if player starts with mechanitor, ensure access to mech research
                    techLevel = TechLevelUtility.Max(techLevel, TechLevel.Ultra);
                }
            }
        }

        return techLevel;
    }

    private static bool IsMechanitorPawnKind(PawnKindDef kindDef)
    {
        return kindDef?.techHediffsRequired?.Contains(ThingDefOf.Mechlink) ?? false;
    }
}
