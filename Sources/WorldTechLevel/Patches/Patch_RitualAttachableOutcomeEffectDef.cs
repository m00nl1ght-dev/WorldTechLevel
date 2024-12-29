using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(RitualAttachableOutcomeEffectDef))]
internal static class Patch_RitualAttachableOutcomeEffectDef
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Quests;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(RitualAttachableOutcomeEffectDef.CanAttachToRitual))]
    internal static void CanAttachToRitual_Postfix(RitualAttachableOutcomeEffectDef __instance, ref AcceptanceReport __result)
    {
        if (__instance.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
