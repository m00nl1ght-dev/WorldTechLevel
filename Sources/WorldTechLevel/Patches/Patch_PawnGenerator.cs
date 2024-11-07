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
