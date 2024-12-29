using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(Dialog_ChooseMemes))]
internal static class Patch_Dialog_ChooseMemes
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Ideoligions;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(Dialog_ChooseMemes.CanUseMeme))]
    internal static void CanUseMeme_Postfix(MemeDef meme, ref bool __result)
    {
        if (meme.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
