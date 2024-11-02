using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(WorldFactionsUIUtility))]
internal static class Patch_WorldFactionsUIUtility
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(WorldFactionsUIUtility.DoWindowContents))]
    private static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("DoWindowContents")
            .MatchLoad(typeof(FactionDefOf), nameof(FactionDefOf.Mechanoid)).Keep()
            .MatchCall(typeof(List<FactionDef>), nameof(List<FactionDef>.Contains)).Keep()
            .Insert(CodeInstruction.Call(typeof(Patch_Page_CreateWorldParams), nameof(Patch_Page_CreateWorldParams.ShouldSkipMechanoidsWarning)));

        return TranspilerPattern.Apply(instructions, pattern);
    }
}
