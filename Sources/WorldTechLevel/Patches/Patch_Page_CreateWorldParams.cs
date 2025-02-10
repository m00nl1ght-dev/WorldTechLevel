using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(Page_CreateWorldParams))]
internal static class Patch_Page_CreateWorldParams
{
    private static readonly List<FactionDef> _removedFactions = [];

    private static bool _confirmedScenarioWarning;
    private static bool _removedPollutionPreIndustrial;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Page_CreateWorldParams.ResetFactionCounts))]
    internal static void ResetFactionCounts_Prefix()
    {
        WorldTechLevel.Current = TechLevel.Archotech;
        _removedFactions.Clear();
        _confirmedScenarioWarning = false;
        _removedPollutionPreIndustrial = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Page_CreateWorldParams.ResetFactionCounts))]
    internal static void ResetFactionCounts_Postfix(ref List<FactionDef> ___factions, ref float ___pollution)
    {
        var scenPart = Find.Scenario?.parts.OfType<ScenPart_WorldTechLevel>().FirstOrDefault();
        var scenarioTechLevel = ResearchUtility.InitialResearchLevelFor(Find.Scenario);

        if (scenPart is { defaultWorldTechLevel: >= TechLevel.Neolithic })
        {
            WorldTechLevel.Current = scenPart.defaultWorldTechLevel;
            ApplyChanges(___factions, ref ___pollution);
        }
        else if (scenarioTechLevel is not TechLevel.Undefined and < TechLevel.Industrial)
        {
            WorldTechLevel.Current = TechLevelUtility.Max(scenarioTechLevel, TechLevel.Neolithic);
            ApplyChanges(___factions, ref ___pollution);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Page_CreateWorldParams.CanDoNext))]
    internal static bool CanDoNext_Prefix(Page __instance, ref bool __result)
    {
        var startingResearch = new List<ResearchProjectDef>();
        var scenarioTechLevel = ResearchUtility.InitialResearchLevelFor(Find.Scenario, startingResearch);

        if (scenarioTechLevel > WorldTechLevel.Current && !_confirmedScenarioWarning)
        {
            var researchStr = startingResearch
                .OrderByDescending(p => p.techLevel)
                .Join(p => $"{p.label.CapitalizeFirst()} ({p.EffectiveTechLevel().ToStringHuman()})", "\n");

            var msg = "WorldTechLevel.ScenarioWarning".Translate(
                Find.Scenario.name, scenarioTechLevel.ToString(), WorldTechLevel.Current.ToString(), researchStr
            );

            Find.WindowStack.Add(new Dialog_MessageBox(msg, "Confirm".Translate(), Confirm, "Cancel".Translate()));

            void Confirm()
            {
                _confirmedScenarioWarning = true;
                __instance.CanDoNext();
            }

            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Page), nameof(Page.DoBack))]
    internal static void DoBack_Prefix(Page __instance)
    {
        if (__instance is Page_CreateWorldParams or Page_SelectScenario)
        {
            WorldTechLevel.Current = TechLevel.Archotech;
            _removedFactions.Clear();
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Page_CreateWorldParams.DoWindowContents))]
    [HarmonyPriority(Priority.VeryLow)]
    private static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var ldlocPos = new CodeInstruction(OpCodes.Ldloc);
        var stlocPos = new CodeInstruction(OpCodes.Stloc);
        var ldlocWidth = new CodeInstruction(OpCodes.Ldloc);

        var patternPos = TranspilerPattern.Build("DoWindowContentsPos")
            .Match(OpCodes.Ldc_R4).Keep()
            .MatchLdloc().StoreOperandIn(ldlocPos, stlocPos).Keep()
            .MatchLdloc().StoreOperandIn(ldlocWidth).Keep()
            .Match(OpCodes.Ldc_R4).Keep()
            .Match(ci => (ConstructorInfo) ci.operand == AccessTools.Constructor(typeof(Rect), [typeof(float), typeof(float), typeof(float), typeof(float)])).Keep()
            .Greedy();

        var pattern = TranspilerPattern.Build("DoWindowContents")
            .OnlyMatchAfter(patternPos)
            .MatchStore(typeof(Page_CreateWorldParams), "population").Keep()
            .Insert(OpCodes.Ldarg_0).Insert(ldlocPos).Insert(ldlocWidth)
            .Insert(CodeInstruction.Call(typeof(Patch_Page_CreateWorldParams), nameof(DoExtraWorldSettings)))
            .Insert(stlocPos);

        return TranspilerPattern.Apply(instructions, patternPos, pattern);
    }

    private static float DoExtraWorldSettings(Page_CreateWorldParams instance, float pos, float width)
    {
        return DoTechLevelSlider(instance.factions, ref instance.pollution, pos, width);
    }

    internal static float DoTechLevelSlider(List<FactionDef> factions, ref float pollution, float pos, float width)
    {
        pos += 40f;

        Widgets.Label(new Rect(0.0f, pos, 200f, 30f), "WorldTechLevel.WorldTechLevel".Translate().CapitalizeFirst());

        var levelBefore = WorldTechLevel.Current;
        var sliderRect = new Rect(200f, pos, width, 30f);

        string currentLabel = levelBefore.SelectionLabel();

        WorldTechLevel.Current = (TechLevel) Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, (float) levelBefore, 2f, 7f, true, currentLabel));

        if (WorldTechLevel.Current != levelBefore)
            ApplyChanges(factions, ref pollution);

        return pos;
    }

    private static void ApplyChanges(List<FactionDef> factions, ref float pollution)
    {
        if (WorldTechLevel.Settings.Filter_Factions)
        {
            var toRemove = factions
                .Where(f => f.EffectiveTechLevel() > WorldTechLevel.Current && f.displayInFactionSelection)
                .ToList();

            foreach (var faction in toRemove)
            {
                factions.Remove(faction);
                _removedFactions.Add(faction);
            }
        }

        var toAdd = _removedFactions
            .Where(f => f.EffectiveTechLevel() <= WorldTechLevel.Current)
            .ToList();

        foreach (var faction in toAdd)
        {
            factions.Add(faction);
            _removedFactions.Remove(faction);
        }

        if (WorldTechLevel.Current < TechLevel.Industrial && !_removedPollutionPreIndustrial)
        {
            _removedPollutionPreIndustrial = true;
            pollution = 0f;
        }
    }
}
