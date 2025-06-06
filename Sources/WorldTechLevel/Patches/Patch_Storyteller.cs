using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(Storyteller))]
internal static class Patch_Storyteller
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Incidents;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(Storyteller.MakeIncidentsForInterval), [typeof(StorytellerComp), typeof(List<IIncidentTarget>)])]
    internal static void MakeIncidentsForInterval_Postfix(ref IEnumerable<FiringIncident> __result)
    {
        if (WorldTechLevel.Current != TechLevel.Archotech)
        {
            __result = __result.Where(IncidentFilter);
        }
    }

    private static bool IncidentFilter(FiringIncident i)
    {
        if (i.parms.questScriptDef?.MinRequiredTechLevel() > WorldTechLevel.Current) return false;
        return i.def.MinRequiredTechLevel() <= WorldTechLevel.Current;
    }
}
