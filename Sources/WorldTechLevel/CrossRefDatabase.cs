using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class CrossRefDatabase
{
    private static readonly Dictionary<Def, List<Def>> CrossRefs = [];

    public static void Register(Def target, Def referencedBy)
    {
        if (!CrossRefs.TryGetValue(target, out var list)) CrossRefs[target] = list = [];
        list.AddDistinct(referencedBy);
    }

    public static IReadOnlyList<Def> AllDefsReferencing(Def def)
    {
        return CrossRefs.TryGetValue(def, out var list) ? list : Array.Empty<Def>();
    }

    public static HashSet<Def> AllAndDependentsAboveTechLevel(TechLevel techLevel)
    {
        var set = new HashSet<Def>();
        var queue = new Queue<Def>();

        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
        {
            if (thingDef.techLevel != TechLevel.Undefined && thingDef.techLevel > techLevel)
            {
                queue.Enqueue(thingDef);
            }
        }

        while (queue.Count > 0)
        {
            var def = queue.Dequeue();
            if (!set.Add(def)) continue;

            if (CrossRefs.TryGetValue(def, out var list))
            {
                Log.Message($"{def.GetType().Name} {def.defName} is referenced by: ");

                foreach (var other in list)
                {
                    if (other is ThingDef) continue;
                    Log.Message($"    {other.GetType().Name} {other.defName}");
                    queue.Enqueue(other);
                }
            }
        }

        return set;
    }

}
