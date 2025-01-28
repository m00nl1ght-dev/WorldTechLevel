using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using WorldTechLevel.Patches;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
public class ModCompat_RealisticPlanets : ModCompat
{
    public override string TargetAssemblyName => "Realistic_Planets_Continued";
    public override string DisplayName => "Realistic Planets Continued";

    private static float _originPos;

    protected override bool OnApply()
    {
        _originPos = 520f;

        if (AccessTools.TypeByName("WorldGenRules.MyLittlePlanet") != null)
        {
            _originPos += 40f;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Planets_Code.Planets_CreateWorldParams", "DoWindowContents")]
    private static void Planets_CreateWorldParams_DoWindowContents_Postfix(ref List<FactionDef> ___factionCounts, ref float ___pollution)
    {
        Patch_Page_CreateWorldParams.DoTechLevelSlider(___factionCounts, ref ___pollution, _originPos, 200f);
    }

    [HarmonyPrefix]
    [HarmonyPatch("Planets_Code.Planets_CreateWorldParams", "ResetFactionCounts")]
    internal static void ResetFactionCounts_Prefix()
    {
        Patch_Page_CreateWorldParams.ResetFactionCounts_Prefix();
    }

    [HarmonyPostfix]
    [HarmonyPatch("Planets_Code.Planets_CreateWorldParams", "ResetFactionCounts")]
    internal static void ResetFactionCounts_Postfix(ref List<FactionDef> ___factionCounts, ref float ___pollution)
    {
        Patch_Page_CreateWorldParams.ResetFactionCounts_Postfix(ref ___factionCounts, ref ___pollution);
    }

    [HarmonyPrefix]
    [HarmonyPatch("Planets_Code.Planets_CreateWorldParams", nameof(Page.CanDoNext))]
    internal static bool CanDoNext_Prefix(Page __instance, ref bool __result)
    {
        return Patch_Page_CreateWorldParams.CanDoNext_Prefix(__instance, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WITab_Planet), nameof(WITab_Planet.FillTab))]
    internal static void FillTab_Prefix(WITab_Planet __instance)
    {
        if (__instance.size.y < 250f)
        {
            __instance.size = new Vector2(__instance.size.x, 250f);
        }
    }
}
