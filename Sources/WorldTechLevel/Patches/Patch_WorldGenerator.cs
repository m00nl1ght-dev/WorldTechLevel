using HarmonyLib;
using LunarFramework.Patching;
using RimWorld.Planet;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(WorldGenerator))]
internal static class Patch_WorldGenerator
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldGenerator.GenerateWorld))]
    internal static void GenerateWorld_Prefix()
    {
        DefFilteringEngine.ApplyTechLevel(Current.Game.TechLevel().WorldTechLevel);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldGenerator.GenerateWithoutWorldData))]
    internal static void GenerateWithoutWorldData_Prefix()
    {
        DefFilteringEngine.ApplyTechLevel(Current.Game.TechLevel().WorldTechLevel);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldGenerator.GenerateFromScribe))]
    internal static void GenerateFromScribe_Prefix()
    {
        DefFilteringEngine.ApplyTechLevel(Current.Game.TechLevel().WorldTechLevel);
    }
}
