using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(RitualAttachableOutcomeEffectDef))]
internal static class Patch_RitualAttachableOutcomeEffectDef
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(RitualAttachableOutcomeEffectDef.CanAttachToRitual))]
    internal static void IsMemeAllowedFor_Postfix(RitualAttachableOutcomeEffectDef __instance, ref AcceptanceReport __result)
    {
        if (__instance.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
