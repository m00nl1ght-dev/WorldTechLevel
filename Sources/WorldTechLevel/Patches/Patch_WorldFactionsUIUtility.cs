using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(WorldFactionsUIUtility))]
internal static class Patch_WorldFactionsUIUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Factions;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(WorldFactionsUIUtility.DoWindowContents))]
    private static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("DoWindowContents")
            .MatchLoad(typeof(FactionDefOf), nameof(FactionDefOf.Mechanoid)).Keep()
            .MatchCall(typeof(List<FactionDef>), nameof(List<FactionDef>.Contains)).Keep()
            .Insert(CodeInstruction.Call(typeof(Patch_WorldFactionsUIUtility), nameof(ShouldSkipMechanoidsWarning)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    internal static bool ShouldSkipMechanoidsWarning(bool present)
    {
        return present || WorldTechLevel.Current < FactionDefOf.Mechanoid.EffectiveTechLevel();
    }
}
