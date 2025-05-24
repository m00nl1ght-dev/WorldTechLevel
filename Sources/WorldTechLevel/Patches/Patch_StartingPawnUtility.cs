using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(StartingPawnUtility))]
internal static class Patch_StartingPawnUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Possessions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(StartingPawnUtility.GeneratePossessions))]
    internal static void GeneratePossessions_Postfix(Pawn pawn)
    {
        if (StartingPawnUtility.StartingPossessions.TryGetValue(pawn, out var list))
            list.RemoveAll(t => t.ThingDef.MinRequiredTechLevel() > pawn.GenFilterTechLevel());
    }
}
