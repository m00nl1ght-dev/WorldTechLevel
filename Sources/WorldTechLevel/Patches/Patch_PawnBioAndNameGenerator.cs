using System.Collections.Generic;
using System.Linq;
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
    [PatchExcludedFromConflictCheck]
    [HarmonyPatch(nameof(PawnBioAndNameGenerator.FillBackstorySlotShuffled))]
    private static IEnumerable<CodeInstruction> FillBackstorySlotShuffled_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("FillBackstorySlotShuffled")
            .Insert(OpCodes.Ldarg_0).Insert(OpCodes.Ldarg_1)
            .InsertCall(typeof(Patch_PawnBioAndNameGenerator), nameof(FilteredBackstories))
            .MatchStloc().Keep()
            .MatchLoad(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.tmpBackstories)).Keep()
            .MatchCall(typeof(List<BackstoryDef>), nameof(List<BackstoryDef>.Clear)).Keep()
            .Greedy(1, 1);

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<BackstoryDef> FilteredBackstories(IEnumerable<BackstoryDef> original, Pawn pawn, BackstorySlot slot)
    {
        if (pawn.story.childhood == null && slot == BackstorySlot.Adulthood)
            return original;

        var filtered = original.FilterByEffectiveTechLevel(pawn.GenFilterTechLevel());
        if (filtered.Any(bs => bs.slot == slot)) return filtered;

        return DefDatabase<BackstoryDef>.AllDefs
            .FilterByEffectiveTechLevel(pawn.GenFilterTechLevel())
            .Where(bs => bs.shuffleable);
    }
}
