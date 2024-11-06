using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(Storyteller))]
internal static class Patch_Storyteller
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(Storyteller.MakeIncidentsForInterval), [typeof(StorytellerComp), typeof(List<IIncidentTarget>)])]
    internal static void MakeIncidentsForInterval_Postfix(ref IEnumerable<FiringIncident> __result)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            __result = __result.Where(i => i.def.EffectiveTechLevel() <= WorldTechLevel.Current);
        }
    }
}
