using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using LunarFramework.Patching;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Main")]
[HarmonyPatch(typeof(DirectXmlLoader))]
internal static class Patch_DirectXmlLoader
{
    private static readonly List<(Def, IList)> wantedLists = [];
    private static readonly List<(Def, IDictionary)> wantedDictionaries = [];
    private static readonly List<(Def, DirectXmlCrossRefLoader.WantedRefForObject)> wantedObjs = [];

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DirectXmlLoader.DefFromNode))]
    private static void DefFromNode_Prefix(out int __state)
    {
        __state = DirectXmlCrossRefLoader.wantedRefs.Count;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(DirectXmlLoader.DefFromNode))]
    private static void DefFromNode_Postfix(Def __result, ref int __state)
    {
        var count = DirectXmlCrossRefLoader.wantedRefs.Count;

        for (int i = __state; i < count; i++)
        {
            var wantedRef = DirectXmlCrossRefLoader.wantedRefs[i];

            if (wantedRef is DirectXmlCrossRefLoader.WantedRefForObject wantedRefObj)
            {
                wantedObjs.Add((__result, wantedRefObj));
            }
            else if (wantedRef.wanter is IList list)
            {
                wantedLists.Add((__result, list));
            }
            else if (wantedRef.wanter is IDictionary dictionary)
            {
                wantedDictionaries.Add((__result, dictionary));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DirectXmlCrossRefLoader), nameof(DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences))]
    private static void ResolveAllWantedCrossReferences_Postfix()
    {
        foreach (var (def, wantedRef) in wantedObjs)
            if (wantedRef.resolvedDef != null)
                CrossRefDatabase.Register(wantedRef.resolvedDef, def);

        foreach (var (def, list) in wantedLists)
            foreach (var obj in list)
                if (obj is Def resolvedDef)
                    CrossRefDatabase.Register(resolvedDef, def);

        foreach (var (def, dict) in wantedDictionaries)
        {
            foreach (var obj in dict.Keys)
                if (obj is Def resolvedDef)
                    CrossRefDatabase.Register(resolvedDef, def);

            foreach (var obj in dict.Values)
                if (obj is Def resolvedDef)
                    CrossRefDatabase.Register(resolvedDef, def);
        }

        wantedObjs.Clear();
        wantedObjs.TrimExcess();
        wantedLists.Clear();
        wantedLists.TrimExcess();
        wantedDictionaries.Clear();
        wantedDictionaries.TrimExcess();
    }
}
