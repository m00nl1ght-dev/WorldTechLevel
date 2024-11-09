using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(PawnGenerator))]
internal static class Patch_PawnGenerator
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnGenerator.GeneratePawn), [typeof(PawnGenerationRequest)])]
    private static void GeneratePawn_Prefix(ref PawnGenerationRequest request)
    {
        var replacement = GetReplacementFor(request);
        if (replacement != null) request = request with { kindDefInner = replacement };
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnGenerator.RedressPawn))]
    private static void RedressPawn_Prefix(ref PawnGenerationRequest request)
    {
        var replacement = GetReplacementFor(request);
        if (replacement != null) request = request with { kindDefInner = replacement };
    }

    private static PawnKindDef GetReplacementFor(PawnGenerationRequest request)
    {
        var kindDef = request.kindDefInner;

        if (kindDef == null || kindDef.EffectiveTechLevel() <= WorldTechLevel.Current) return null;
        if (request.Context == PawnGenerationContext.PlayerStarter) return null;

        var alternative = kindDef.GetAlternative();

        if (alternative != null) WorldTechLevel.Logger.Log($"Generating pawn of kind {alternative.defName} in place of {kindDef.defName}");
        else WorldTechLevel.Logger.Warn($"No valid alternative found for {kindDef.EffectiveTechLevel()} tech level pawn kind {kindDef.defName}");

        return alternative;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PawnGenerator.GenerateTraitsFor))]
    private static IEnumerable<CodeInstruction> GenerateTraitsFor_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("GenerateTraitsFor")
            .MatchCall(typeof(DefDatabase<TraitDef>), "get_AllDefsListForReading")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnGenerator), nameof(FilteredTraits)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<TraitDef> FilteredTraits()
    {
        return DefDatabase<TraitDef>.AllDefs.FilterByEffectiveTechLevel();
    }
}
