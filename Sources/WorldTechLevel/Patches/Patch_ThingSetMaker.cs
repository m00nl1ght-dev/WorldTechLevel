using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(ThingSetMaker))]
internal static class Patch_ThingSetMaker
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ThingSetMaker.Generate), [typeof(ThingSetMakerParams)])]
    internal static void Generate_Postfix(List<Thing> __result)
    {
        __result.RemoveAll(t => t.def.EffectiveTechLevel() > WorldTechLevel.Current);
    }
}
