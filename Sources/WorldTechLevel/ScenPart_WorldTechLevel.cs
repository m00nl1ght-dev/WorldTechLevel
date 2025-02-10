using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public class ScenPart_WorldTechLevel : ScenPart
{
    public TechLevel defaultWorldTechLevel = TechLevel.Archotech;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref this.defaultWorldTechLevel, "defaultWorldTechLevel", TechLevel.Archotech);
    }

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        if (Widgets.ButtonText(listing.GetScenPartRect(this, RowHeight), this.defaultWorldTechLevel.SelectionLabel()))
        {
            var options = new List<FloatMenuOption>();

            foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
            {
                var techLevelLocal = techLevel;
                if (techLevelLocal <= TechLevel.Animal) continue;
                options.Add(new FloatMenuOption(techLevelLocal.SelectionLabel(), () => this.defaultWorldTechLevel = techLevelLocal));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    public override string Summary(Scenario scen) => "";

    public override int GetHashCode() => base.GetHashCode() ^ this.defaultWorldTechLevel.GetHashCode();
}
