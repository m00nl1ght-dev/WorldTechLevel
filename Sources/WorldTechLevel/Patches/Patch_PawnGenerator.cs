using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnGenerator))]
internal static class Patch_PawnGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled(MethodBase original)
    {
        if (original == null)
            return true;

        var settings = WorldTechLevel.Settings;

        if (original.Name == nameof(PawnGenerator.GenerateGearFor))
            return settings.Filter_Weapons || settings.Filter_Apparel || settings.Filter_Possessions;

        if (original.Name == nameof(PawnGenerator.GenerateTraitsFor))
            return settings.Filter_Traits;

        if (original.Name == nameof(PawnGenerator.XenotypesAvailableFor))
            return settings.Filter_Xenotypes;

        if (original.Name == nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn))
            return settings.Filter_Xenotypes;

        return settings.Filter_PawnKinds;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [PatchExcludedFromConflictCheck]
    [HarmonyPatch(nameof(PawnGenerator.GeneratePawn), [typeof(PawnGenerationRequest)])]
    private static void GeneratePawn_Prefix(ref PawnGenerationRequest request)
    {
        var replacement = GetReplacementFor(request);
        if (replacement != null) request = request with { kindDefInner = replacement };
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [PatchExcludedFromConflictCheck]
    [HarmonyPatch(nameof(PawnGenerator.RedressPawn))]
    private static void RedressPawn_Prefix(ref PawnGenerationRequest request)
    {
        var replacement = GetReplacementFor(request);
        if (replacement != null) request = request with { kindDefInner = replacement };
    }

    private static PawnKindDef GetReplacementFor(PawnGenerationRequest request)
    {
        var kindDef = request.kindDefInner;
        var filterLevel = request.Faction.CurrentFilterLevel();

        if (kindDef == null || kindDef.MinRequiredTechLevel() <= filterLevel) return null;
        if (request.Context == PawnGenerationContext.PlayerStarter) return null;

        var minLevel = TechLevelUtility.Min(filterLevel, TechLevel.Medieval);
        var alternative = kindDef.GetAlternative(filterLevel, minLevel);

        if (alternative != null) WorldTechLevel.Logger.Debug($"Generating pawn of kind {alternative.defName} in place of {kindDef.defName}");
        else WorldTechLevel.Logger.Debug($"No alternative found for {kindDef.MinRequiredTechLevel()} tech level pawn kind {kindDef.defName}");

        return alternative;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnGenerator.GenerateGearFor))]
    private static void GenerateGearFor_Postfix(Pawn pawn)
    {
        if (pawn.IsStartingPawnGen()) return;

        var filterLevel = pawn.Faction.CurrentFilterLevel();

        if (WorldTechLevel.Settings.Filter_Possessions && pawn.inventory != null)
        {
            var toFilter = pawn.inventory.innerContainer
                .Where(t => t.MinRequiredTechLevel() > filterLevel).ToList();

            foreach (var thing in toFilter)
                pawn.inventory.innerContainer.Remove(thing);

            foreach (var thing in toFilter)
                if (ReplacementUtility.TryMakeReplacementFor(thing, pawn) is { } replacement)
                    pawn.inventory.innerContainer.TryAdd(replacement);
        }

        if (WorldTechLevel.Settings.Filter_Apparel && pawn.apparel != null)
        {
            var toFilter = pawn.apparel.WornApparel
                .Where(t => t.MinRequiredTechLevel() > filterLevel).ToList();

            foreach (var apparel in toFilter)
                pawn.apparel.Remove(apparel);

            foreach (var apparel in toFilter)
                if (ReplacementUtility.TryMakeReplacementFor(apparel, pawn) is Apparel replacement)
                    pawn.apparel.Wear(replacement, false);
        }

        if (WorldTechLevel.Settings.Filter_Weapons && pawn.equipment != null)
        {
            var toFilter = pawn.equipment.AllEquipmentListForReading
                .Where(t => t.MinRequiredTechLevel() > filterLevel).ToList();

            foreach (var equipment in toFilter)
                pawn.equipment.Remove(equipment);

            foreach (var equipment in toFilter)
                if (ReplacementUtility.TryMakeReplacementFor(equipment, pawn) is ThingWithComps replacement)
                    pawn.equipment.AddEquipment(replacement);
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PawnGenerator.GenerateTraitsFor))]
    private static IEnumerable<CodeInstruction> GenerateTraitsFor_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("GenerateTraitsFor")
            .Insert(OpCodes.Ldarg_0)
            .MatchCall(typeof(DefDatabase<TraitDef>), "get_AllDefsListForReading")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnGenerator), nameof(FilteredTraits)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<TraitDef> FilteredTraits(Pawn pawn)
    {
        return DefDatabase<TraitDef>.AllDefs.FilterByMinRequiredTechLevel(pawn.GenFilterTechLevel());
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PawnGenerator.XenotypesAvailableFor))]
    private static void XenotypesAvailableFor_Postfix(Dictionary<XenotypeDef, float> __result, FactionDef factionDef)
    {
        var filterLevel = factionDef.CurrentFilterLevel();
        __result.RemoveAll(kv => kv.Key.MinRequiredTechLevel() > filterLevel);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn))]
    private static void AdjustXenotypeForFactionlessPawn_Postfix(ref XenotypeDef xenotype)
    {
        if (xenotype.MinRequiredTechLevel() > WorldTechLevel.Current)
            xenotype = XenotypeDefOf.Baseliner;
    }
}
