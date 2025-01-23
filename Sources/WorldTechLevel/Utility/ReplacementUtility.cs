using System;
using System.Linq;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class ReplacementUtility
{
    public static T GetAlternative<T>(this T def) where T : Def => def.GetAlternative(WorldTechLevel.Current);

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
            var level = option.def.EffectiveTechLevel();
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

        var minLevel = TechLevelUtility.Min(WorldTechLevel.Current, TechLevel.Medieval);

        if (thing.def.EffectiveTechLevel() > WorldTechLevel.Current)
            newDef = thing.def.GetAlternative(WorldTechLevel.Current, minLevel, owner == null ? null : ApparelValidator);

        if (newDef == null)
        {
            WorldTechLevel.Logger.Log($"No replacement found for {thing.def} [{thing.Stuff}]");
            return null;
        }

        var newStuff = AppropriateStuffFor(newDef, owner);
        var newThing = ThingMaker.MakeThing(newDef, newStuff);
        newThing.stackCount = Math.Min(thing.stackCount, newDef.stackLimit);

        WorldTechLevel.Logger.Log($"Replacement for {thing.def} [{thing.Stuff}] -> {newThing.def} [{newThing.Stuff}]");

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
            return def.apparel.PawnCanWear(owner);
        }

        return newThing;
    }

    public static ThingDef AppropriateStuffFor(ThingDef def, Pawn owner = null)
    {
        if (!def.MadeFromStuff) return null;

        bool Validator(ThingDef stuff)
        {
            if (stuff.EffectiveTechLevel() >= WorldTechLevel.Current) return false;
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
}
