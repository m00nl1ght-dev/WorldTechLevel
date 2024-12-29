using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(PawnBioAndNameGenerator))]
internal static class Patch_PawnBioAndNameGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Backstories;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(PawnBioAndNameGenerator.TryGetRandomUnusedSolidBioFor))]
    internal static bool TryGetRandomUnusedSolidBioFor_Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PawnBioAndNameGenerator.FillBackstorySlotShuffled))]
    private static IEnumerable<CodeInstruction> FillBackstorySlotShuffled_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("FillBackstorySlotShuffled")
            .Insert(OpCodes.Ldarg_0)
            .MatchCall(typeof(DefDatabase<BackstoryDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnBioAndNameGenerator), nameof(FilteredBackstories)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<BackstoryDef> FilteredBackstories(Pawn pawn)
    {
        return DefDatabase<BackstoryDef>.AllDefs.FilterByEffectiveTechLevel(pawn.GenFilterTechLevel());
    }
}
