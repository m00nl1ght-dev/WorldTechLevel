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