using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(IdeoUtility))]
internal static class Patch_IdeoUtility
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Ideoligions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(IdeoUtility.IsMemeAllowedFor))]
    internal static void IsMemeAllowedFor_Postfix(MemeDef meme, ref bool __result)
    {
        if (meme.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
