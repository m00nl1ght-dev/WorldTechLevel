using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(StockGenerator))]
internal static class Patch_StockGenerator
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Items;

    [HarmonyTargetMethods]
    private static IEnumerable<MethodInfo> TargetMethods()
    {
        yield return AccessTools.Method(typeof(StockGenerator_Category), nameof(StockGenerator.GenerateThings));
        yield return AccessTools.Method(typeof(StockGenerator_MiscItems), nameof(StockGenerator.GenerateThings));
        yield return AccessTools.Method(typeof(StockGenerator_Tag), nameof(StockGenerator.GenerateThings));
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    private static void GenerateThings_Prefix(StockGenerator __instance, ref State __state)
    {
        __state.maxTechLevelBuy = __instance.maxTechLevelBuy;
        __state.maxTechLevelGenerate = __instance.maxTechLevelGenerate;

        if (__instance.trader is { orbital: true } && WorldTechLevel.Settings.AlwaysAllowOffworld) return;

        if (__instance.maxTechLevelBuy > WorldTechLevel.Current)
            __instance.maxTechLevelBuy = WorldTechLevel.Current;

        if (__instance.maxTechLevelGenerate > WorldTechLevel.Current)
            __instance.maxTechLevelGenerate = WorldTechLevel.Current;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    private static void GenerateThings_Postfix(StockGenerator __instance, ref State __state)
    {
        __instance.maxTechLevelBuy = __state.maxTechLevelBuy;
        __instance.maxTechLevelGenerate = __state.maxTechLevelGenerate;
    }

    private struct State
    {
        public TechLevel maxTechLevelGenerate;
        public TechLevel maxTechLevelBuy;
    }
}
