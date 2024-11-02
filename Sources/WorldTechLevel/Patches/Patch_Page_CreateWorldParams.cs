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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Page_CreateWorldParams.ResetFactionCounts))]
    internal static void ResetFactionCounts_Prefix()
    {
        WorldTechLevel.Current = TechLevel.Archotech;
        _removedFactions.Clear();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Page), nameof(Page.DoBack))]
    internal static void DoBack_Prefix(Page __instance)
    {
        if (__instance is not Page_CreateWorldParams) return;
        WorldTechLevel.Current = TechLevel.Archotech;
        _removedFactions.Clear();
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
            .Insert(CodeInstruction.Call(typeof(Patch_Page_CreateWorldParams), nameof(DoExtraSliders)))
            .Insert(stlocPos);

        return TranspilerPattern.Apply(instructions, patternPos, pattern);
    }

    internal static float DoExtraSliders(Page_CreateWorldParams instance, float pos, float width)
    {
        pos += 40f;

        Widgets.Label(new Rect(0.0f, pos, 200f, 30f), "WorldTechLevel.Settings.TechLevel".Translate().CapitalizeFirst());

        var levelBefore = WorldTechLevel.Current;
        var sliderRect = new Rect(200f, pos, width, 30f);
        var currentLabel = levelBefore.ToStringHuman().CapitalizeFirst();

        WorldTechLevel.Current = (TechLevel) Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, (float) levelBefore, 2f, 7f, true, currentLabel));

        if (WorldTechLevel.Current < levelBefore)
        {
            var toRemove = instance.factions.Where(f => f.techLevel > WorldTechLevel.Current && f.displayInFactionSelection).ToList();

            foreach (var faction in toRemove)
            {
                instance.factions.Remove(faction);
                _removedFactions.Add(faction);
            }
        }
        else if (WorldTechLevel.Current > levelBefore)
        {
            var toAdd = _removedFactions.Where(f => f.techLevel <= WorldTechLevel.Current).ToList();

            foreach (var faction in toAdd)
            {
                instance.factions.Add(faction);
                _removedFactions.Remove(faction);
            }
        }

        return pos;
    }
}
