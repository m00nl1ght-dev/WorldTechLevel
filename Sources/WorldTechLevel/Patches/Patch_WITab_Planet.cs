using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(WITab_Planet))]
internal static class Patch_WITab_Planet
{
    [HarmonyPostfix]
    [HarmonyPatch("get_Desc")]
    internal static void GetDesc_Postfix(ref string __result)
    {
        __result += "WorldTechLevel.TechLevel".Translate().CapitalizeFirst() + ": " + WorldTechLevel.Current.SelectionLabel() + "\n";
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(WITab_Planet.FillTab))]
    internal static void FillTab_Postfix()
    {
        var rect = new Rect(10f, WITab_Planet.WinSize.y - 40f, 200f, 30f);

        if (Widgets.ButtonText(rect, "WorldTechLevel.ChangeTechLevel".Translate().CapitalizeFirst()))
        {
            var options = new List<FloatMenuOption>();

            foreach (var value in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
            {
                if (value > TechLevel.Animal)
                {
                    options.Add(new FloatMenuOption(value.SelectionLabel(), SetLevel));

                    void SetLevel()
                    {
                        WorldTechLevel.Current = value;
                        Current.Game.TechLevel().WorldTechLevel = value;
                    }
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}