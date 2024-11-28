using System.Linq;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(QuestNode_Root_Mission_BanditCamp))]
internal static class Patch_QuestNode_Root_Mission_BanditCamp
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(QuestNode_Root_Mission_BanditCamp.TryGetSiteFaction))]
    internal static void TryGetSiteFaction_Postfix(ref Faction faction, ref bool __result)
    {
        if (!__result && Find.FactionManager.AllFactions.Where(f => !f.temporary && f.HostileTo(Faction.OfPlayer)).TryRandomElement(out faction))
        {
            WorldTechLevel.Logger.Log($"Selecting faction {faction.name} for QuestNode_Root_Mission_BanditCamp");
            __result = true;
        }
    }
}
