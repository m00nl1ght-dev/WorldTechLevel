using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_RealRuins : ModCompat
{
    public override string TargetAssemblyName => "RealRuins";
    public override string DisplayName => "Real Ruins";

    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_GenSteps;

    [HarmonyTranspiler]
    [HarmonyPatch("RealRuins.BlueprintTransferUtility", "RemoveIncompatibleItems")]
    private static IEnumerable<CodeInstruction> RemoveIncompatibleItems_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var pattern1 = TranspilerPattern.Build("TerrainDef")
            .MatchCall(typeof(DefDatabase<TerrainDef>), nameof(DefDatabase<TerrainDef>.GetNamed), [typeof(string), typeof(bool)])
            .Replace(OpCodes.Call, AccessTools.Method(typeof(ModCompat_RealRuins), nameof(GetNamedTerrainDef)));

        var pattern2 = TranspilerPattern.Build("ThingDef")
            .MatchCall(typeof(DefDatabase<ThingDef>), nameof(DefDatabase<ThingDef>.GetNamed), [typeof(string), typeof(bool)])
            .Replace(OpCodes.Call, AccessTools.Method(typeof(ModCompat_RealRuins), nameof(GetNamedThingDef)));

        return TranspilerPattern.Apply(instructions, pattern1, pattern2);
    }

    private static TerrainDef GetNamedTerrainDef(string defName, bool errorOnFail)
    {
        var terrainDef = DefDatabase<TerrainDef>.GetNamed(defName, errorOnFail);
        if (terrainDef != null && terrainDef.EffectiveTechLevel() > WorldTechLevel.Current) return null;
        return terrainDef;
    }

    private static ThingDef GetNamedThingDef(string defName, bool errorOnFail)
    {
        var thingDef = DefDatabase<ThingDef>.GetNamed(defName, errorOnFail);
        if (thingDef != null && thingDef.EffectiveTechLevel() > WorldTechLevel.Current) return null;
        return thingDef;
    }
}
