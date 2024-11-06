using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(IdeoUtility))]
internal static class Patch_IdeoUtility
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IdeoUtility.IsMemeAllowedFor))]
    internal static void IsMemeAllowedFor_Postfix(MemeDef meme, ref bool __result)
    {
        if (meme.EffectiveTechLevel() > WorldTechLevel.Current) __result = false;
    }
}
