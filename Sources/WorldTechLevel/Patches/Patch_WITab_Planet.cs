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
    [HarmonyPriority(Priority.Low)]
    internal static void GetDesc_Postfix(ref string __result)
    {
        __result += "WorldTechLevel.TechLevel".Translate().CapitalizeFirst() + ": " + WorldTechLevel.Current.SelectionLabel() + "\n";
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(WITab_Planet.FillTab))]
    internal static void FillTab_Postfix(WITab_Planet __instance)
    {
        if (Current.ProgramState != ProgramState.Playing) return;

        var rect = new Rect(10f, __instance.size.y - 40f, 200f, 30f);

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
                        var prevLevel = WorldTechLevel.Current;

                        WorldTechLevel.Current = value;
                        Current.Game.TechLevel().WorldTechLevel = value;

                        WorldTechLevelSettings.RefreshResearchViewWidth();
                        Window_AddFactions.OpenIfAnyAvailable(prevLevel);
                    }
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
