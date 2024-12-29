using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(BiomeDef))]
internal static class Patch_BiomeDef
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Diseases;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BiomeDef.CommonalityOfDisease))]
    private static void CommonalityOfDisease_Postfix(IncidentDef diseaseInc, ref float __result)
    {
        if (diseaseInc.EffectiveTechLevel() > WorldTechLevel.Current)
        {
            __result = 0f;
        }
    }
}
