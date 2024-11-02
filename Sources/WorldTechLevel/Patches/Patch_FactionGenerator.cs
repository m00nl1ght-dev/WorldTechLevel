using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(FactionGenerator))]
internal static class Patch_FactionGenerator
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FactionGenerator.ConfigurableFactions), MethodType.Getter)]
    internal static void GetConfigurableFactions(ref IEnumerable<FactionDef> __result)
    {
        __result = __result.Where(Patch_Page_CreateWorldParams.CanAddFaction);
    }
}
