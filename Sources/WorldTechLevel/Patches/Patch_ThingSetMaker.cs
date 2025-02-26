using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(ThingSetMaker))]
internal static class Patch_ThingSetMaker
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled(MethodBase original)
    {
        if (original == null)
            return true;

        if (original.DeclaringType == typeof(ThingSetMaker_Meteorite))
            return WorldTechLevel.Settings.Filter_MineableResources;

        if (original.DeclaringType == typeof(ThingSetMaker_Pawn))
            return WorldTechLevel.Settings.Filter_PawnKinds;

        return WorldTechLevel.Settings.Filter_Items;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(ThingSetMaker.Generate), [typeof(ThingSetMakerParams)])]
    internal static void Generate_Prefix(ref ThingSetMakerParams parms)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            if (parms.techLevel is null or TechLevel.Undefined || parms.techLevel > WorldTechLevel.Current)
            {
                parms.techLevel = WorldTechLevel.Current;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThingSetMaker_Books), nameof(ThingSetMaker.CanGenerateSub))]
    internal static void Books_CanGenerateSub_Postfix(ref bool __result)
    {
        if (WorldTechLevel.Current < TechLevel.Medieval) __result = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThingSetMaker_Meteorite), nameof(ThingSetMaker_Meteorite.FindRandomMineableDef))]
    internal static void Meteorite_FindRandomMineableDef_Postfix(ref ThingDef __result)
    {
        if (__result.building?.mineableThing?.EffectiveTechLevel() > WorldTechLevel.Current)
        {
            __result = ThingSetMaker_Meteorite.nonSmoothedMineables
                .Where(t => t.building?.mineableThing?.EffectiveTechLevel() <= WorldTechLevel.Current)
                .RandomElement() ?? ThingSetMaker_Meteorite.nonSmoothedMineables[0];
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThingSetMaker_Pawn), nameof(ThingSetMaker.Generate))]
    internal static bool Pawn_Generate_Prefix(ThingSetMaker_Pawn __instance)
    {
        return __instance.pawnKind.EffectiveTechLevel() <= WorldTechLevel.Current;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), nameof(ThingSetMaker_MapGen_AncientPodContents.MakeIntoContainer))]
    internal static bool AncientPodContents_MakeIntoContainer_Prefix(ThingDef def)
    {
        return def.EffectiveTechLevel() <= WorldTechLevel.Current;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThingSetMaker_RandomGeneralGoods), nameof(ThingSetMaker_RandomGeneralGoods.RandomMeals))]
    internal static void RandomGeneralGoods_RandomMeals_Postfix(ref Thing __result)
    {
        if (__result.def.EffectiveTechLevel() > WorldTechLevel.Current) __result = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThingSetMaker_RandomGeneralGoods), nameof(ThingSetMaker_RandomGeneralGoods.RandomMedicine))]
    internal static void RandomGeneralGoods_RandomMedicine_Postfix(ref Thing __result)
    {
        if (__result.def.EffectiveTechLevel() > WorldTechLevel.Current) __result = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThingSetMaker_ResourcePod), nameof(ThingSetMaker_ResourcePod.PossiblePodContentsDefs))]
    internal static void ResourcePod_PossiblePodContentsDefs_Postfix(ref IEnumerable<ThingDef> __result)
    {
        __result = __result.FilterByEffectiveTechLevel();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThingSetMaker_TraderStock), nameof(ThingSetMaker.Generate))]
    internal static void TraderStock_Generate_Postfix(ThingSetMakerParams parms, List<Thing> outThings)
    {
        if (parms.traderDef is { orbital: true } && WorldTechLevel.Settings.AlwaysAllowOffworld) return;

        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            outThings.RemoveAll(t => t.def.EffectiveTechLevel() > WorldTechLevel.Current);
        }
    }
}
