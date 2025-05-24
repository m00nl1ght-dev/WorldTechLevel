using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(GenStep_ScatterLumpsMineable))]
internal static class Patch_GenStep_ScatterLumpsMineable
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_MineableResources;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(GenStep_ScatterLumpsMineable.ChooseThingDef))]
    private static IEnumerable<CodeInstruction> ChooseThingDef_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("ChooseThingDef")
            .MatchCall(typeof(DefDatabase<ThingDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_GenStep_ScatterLumpsMineable), nameof(FilteredThings)));

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<ThingDef> FilteredThings()
    {
        return DefDatabase<ThingDef>.AllDefs.Where(
            def => def.building?.mineableThing == null || def.building.mineableThing.MinRequiredTechLevel() <= WorldTechLevel.Current
        );
    }
}
