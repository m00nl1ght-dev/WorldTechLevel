using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(HealthUtility))]
internal static class Patch_HealthUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_DamageTypes;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(HealthUtility.RandomViolenceDamageType))]
    internal static void RandomViolenceDamageType_Postfix(ref DamageDef __result)
    {
        if (__result == DamageDefOf.Bullet && WorldTechLevel.Current < TechLevel.Industrial)
            __result = DamageDefOf.Cut;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(HealthUtility.RandomPermanentInjuryDamageType))]
    internal static void RandomPermanentInjuryDamageType_Postfix(ref DamageDef __result)
    {
        if (__result == DamageDefOf.Bullet && WorldTechLevel.Current < TechLevel.Industrial)
            __result = DamageDefOf.Stab;
    }
}
