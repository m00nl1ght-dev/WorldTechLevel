using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(StartingPawnUtility))]
internal static class Patch_StartingPawnUtility
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(StartingPawnUtility.GeneratePossessions))]
    internal static void GeneratePossessions_Postfix(Pawn pawn)
    {
        if (pawn.Faction != null && pawn.Faction.def.techLevel != TechLevel.Undefined && pawn.Faction.def.techLevel <= WorldTechLevel.Current)
        {
            if (StartingPawnUtility.StartingPossessions.TryGetValue(pawn, out var list))
            {
                list.RemoveAll(t => t.ThingDef.EffectiveTechLevel() > WorldTechLevel.Current);
            }
        }
    }
}
