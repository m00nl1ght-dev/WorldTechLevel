using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(QuestNode_Root_Mission_BanditCamp))]
internal static class Patch_QuestNode_Root_Mission_BanditCamp
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_Factions;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(QuestNode_Root_Mission_BanditCamp.TryGetSiteFaction))]
    internal static void TryGetSiteFaction_Postfix(ref Faction faction, ref bool __result)
    {
        if (!__result && Find.FactionManager.AllFactions.Where(CanUseFaction).TryRandomElement(out faction))
        {
            WorldTechLevel.Logger.Log($"Selecting faction {faction.name} for QuestNode_Root_Mission_BanditCamp");
            __result = true;
        }
    }

    private static bool CanUseFaction(Faction faction)
    {
        if (faction.temporary || !faction.HostileTo(Faction.OfPlayer)) return false;
        return faction.def.pawnGroupMakers?.Any(p => p.kindDef == PawnGroupKindDefOf.Settlement) ?? false;
    }
}
