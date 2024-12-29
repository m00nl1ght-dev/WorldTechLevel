using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(Page_ChooseIdeoPreset))]
internal static class Patch_Page_ChooseIdeoPreset
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Ideoligions;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Page_ChooseIdeoPreset.DrawCategory))]
    private static IEnumerable<CodeInstruction> DrawCategory_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern = TranspilerPattern.Build("DrawCategory")
            .MatchCall(typeof(DefDatabase<IdeoPresetDef>), "get_AllDefs")
            .Replace(OpCodes.Call, AccessTools.Method(typeof(Patch_Page_ChooseIdeoPreset), nameof(FilteredPresets)))
            .Greedy();

        return TranspilerPattern.Apply(instructions, pattern);
    }

    private static IEnumerable<IdeoPresetDef> FilteredPresets()
    {
        return DefDatabase<IdeoPresetDef>.AllDefs.FilterByEffectiveTechLevel();
    }
}
