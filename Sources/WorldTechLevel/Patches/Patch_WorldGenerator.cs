using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld.Planet;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(WorldGenerator))]
internal static class Patch_WorldGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled(MethodBase original)
    {
        if (original == null)
            return true;

        if (original.Name == "get_GenStepsInOrder")
            return WorldTechLevel.Settings.Filter_WorldGenSteps;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(WorldGenerator.GenStepsInOrder), MethodType.Getter)]
    private static void GetGenStepsInOrder_Postfix(ref IEnumerable<WorldGenStepDef> __result)
    {
        __result = __result.FilterByEffectiveTechLevel();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldGenerator.GenerateWorld))]
    internal static void GenerateWorld_Prefix()
    {
        Current.Game.TechLevel().WorldTechLevel = WorldTechLevel.Current;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldGenerator.GenerateWithoutWorldData))]
    internal static void GenerateWithoutWorldData_Prefix()
    {
        WorldTechLevel.Current = Current.Game.TechLevel().WorldTechLevel;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldGenerator.GenerateFromScribe))]
    internal static void GenerateFromScribe_Prefix()
    {
        WorldTechLevel.Current = Current.Game.TechLevel().WorldTechLevel;
    }

    internal static GameComponent_TechLevel TechLevel(this Game game)
    {
        return game.GetComponent<GameComponent_TechLevel>();
    }
}
