using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class ReplacementUtility
{
    public static T GetAlternative<T>(
        this T def, TechLevel targetLevel,
        TechLevel minLevel = TechLevel.Neolithic,
        Predicate<T> validator = null) where T : Def
    {
        TechLevelDatabase<T>.EnsureInitialized();

        var data = TechLevelDatabase<T>.Alternatives;
        if (def.index >= data.Length) return null;

        var alternatives = data[def.index];
        if (alternatives == null) return null;

        bool Filter(TechLevelDatabase<T>.Alternative option)
        {
            var level = option.def.MinRequiredTechLevel();
            if (level != targetLevel && level != TechLevel.Undefined) return false;
            return validator == null || validator(option.def);
        }

        while (!alternatives.Any(Filter))
        {
            if (targetLevel <= minLevel) return null;
            targetLevel -= 1;
        }

        return alternatives.Where(Filter).RandomElementByWeight(a => a.weight).def;
    }

    public static Thing TryMakeReplacementFor(Thing thing, Pawn owner = null)
    {
        var newDef = thing.def;
        var minLevel = TechLevel.Undefined;

        if (thing.def.IsApparel && WorldTechLevel.Current > TechLevel.Neolithic)
            minLevel = TechLevel.Medieval;

        if (thing.def.MinRequiredTechLevel() > WorldTechLevel.Current)
            newDef = thing.def.GetAlternative(WorldTechLevel.Current, minLevel, owner == null ? null : ApparelValidator);

        if (newDef == null)
        {
            WorldTechLevel.Logger.Debug($"No replacement found for {thing.def} [{thing.Stuff}]");
            return null;
        }

        var newStuff = AppropriateStuffFor(newDef, owner);
        var newThing = ThingMaker.MakeThing(newDef, newStuff);
        newThing.stackCount = Math.Min(thing.stackCount, newDef.stackLimit);

        WorldTechLevel.Logger.Debug($"Replacement for {thing.def} [{thing.Stuff}] -> {newThing.def} [{newThing.Stuff}]");

        if (thing.TryGetComp<CompQuality>(out var quality) && newThing.TryGetComp<CompQuality>(out var newQuality))
            newQuality.SetQuality(quality.Quality, ArtGenerationContext.Outsider);

        if (owner != null)
        {
            if (newThing is Apparel apparel)
            {
                PawnGenerator.PostProcessGeneratedGear(apparel, owner);
                PawnApparelGenerator.PostProcessApparel(apparel, owner);
            }
            else if (newThing.HasComp<CompEquippable>())
            {
                PawnGenerator.PostProcessGeneratedGear(newThing, owner);
                newThing.StyleDef = owner.kindDef.weaponStyleDef ?? owner.Ideo?.GetStyleFor(newThing.def);
            }
        }

        bool ApparelValidator(ThingDef def)
        {
            if (!def.IsApparel) return true;
            if (!thing.def.IsApparel) return false;
            if (!ApparelUtility.HasPartsToWear(owner, def)) return false;
            if (!owner.apparel.CanWearWithoutDroppingAnything(def)) return false;
            if (!thing.def.apparel.bodyPartGroups.Intersect(def.apparel.bodyPartGroups).Any()) return false;
            if (!thing.def.apparel.layers.Intersect(def.apparel.layers).Any()) return false;
            return def.apparel.PawnCanWear(owner);
        }

        return newThing;
    }

    public static IEnumerable<T> FilterWithAlternatives<T>(this IEnumerable<T> source, TechLevel techLevel) where T : Def
    {
        TechLevelDatabase<T>.EnsureInitialized();

        foreach (var original in source)
        {
            if (original.MinRequiredTechLevel() <= techLevel)
            {
                yield return original;
            }
            else if (original.GetAlternative(techLevel) is { } replacement && !source.Contains(replacement))
            {
                yield return replacement;
            }
        }
    }

    public static ThingDef AppropriateStuffFor(ThingDef def, Pawn owner = null)
    {
        if (!def.MadeFromStuff) return null;

        bool Validator(ThingDef stuff)
        {
            if (stuff.MinRequiredTechLevel() >= WorldTechLevel.Current) return false;
            if (owner == null || !def.IsApparel) return true;
            return PawnApparelGenerator.CanUseStuff(owner, new ThingStuffPair { thing = def, stuff = stuff });
        }

        if (!GenStuff.AllowedStuffsFor(def).Where(Validator).TryRandomElementByWeight(s => s.stuffProps.commonality, out var stuff))
            stuff = GenStuff.DefaultStuffFor(def);

        return stuff;
    }

    public static bool IsStartingPawnGen(this Pawn pawn)
    {
        return Current.ProgramState == ProgramState.Entry && pawn.Faction is { IsPlayer: true };
    }

    public static Regex GlobMatcher(this string pattern)
    {
        var regex = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".");
        return new Regex("^" + regex + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }
}
