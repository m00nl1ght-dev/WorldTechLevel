using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(FactionGenerator))]
internal static class Patch_FactionGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Factions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(FactionGenerator.ConfigurableFactions), MethodType.Getter)]
    internal static void GetConfigurableFactions(ref IEnumerable<FactionDef> __result)
    {
        __result = __result.Where(f => f.MinRequiredTechLevel() <= WorldTechLevel.Current);
    }

    [HarmonyTranspiler]
    #if RW_1_6_OR_GREATER
    [HarmonyPatch(nameof(FactionGenerator.InitializeFactions))]
    #else
    [HarmonyPatch(nameof(FactionGenerator.GenerateFactionsIntoWorld))]
    #endif
    private static IEnumerable<CodeInstruction> InitializeFactions_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("InitializeFactions")
            .MatchCall(typeof(DefDatabase<FactionDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_FactionGenerator), nameof(FilteredFactions)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<FactionDef> FilteredFactions()
    {
        return DefDatabase<FactionDef>.AllDefs.Where(def => def.MinRequiredTechLevel() <= WorldTechLevel.Current);
    }
}
