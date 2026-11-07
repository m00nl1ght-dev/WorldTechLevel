using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

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

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    private static void GenerateThings_Postfix(StockGenerator __instance, Faction faction, ref IEnumerable<Thing> __result)
    {
        if (__result == null) return;
        if (__instance.trader is { orbital: true } && WorldTechLevel.Settings.AlwaysAllowOffworld) return;

        __result = GenerateThingsClamped(__result, __instance, faction.CurrentFilterLevel());
    }

    private static IEnumerable<Thing> GenerateThingsClamped(IEnumerable<Thing> things, StockGenerator generator, TechLevel filterLevel)
    {
        var maxTechLevelBuy = generator.maxTechLevelBuy;
        var maxTechLevelGenerate = generator.maxTechLevelGenerate;

        if (maxTechLevelBuy > filterLevel)
            generator.maxTechLevelBuy = filterLevel;

        if (maxTechLevelGenerate > filterLevel)
            generator.maxTechLevelGenerate = filterLevel;

        try
        {
            foreach (var thing in things) yield return thing;
        }
        finally
        {
            generator.maxTechLevelBuy = maxTechLevelBuy;
            generator.maxTechLevelGenerate = maxTechLevelGenerate;
        }
    }
}
