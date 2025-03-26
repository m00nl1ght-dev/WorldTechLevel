using System.Linq;
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
            var options = TechLevelUtility.AllSelectableTechLevels
                .Select(tl => new FloatMenuOption(tl.SelectionLabel(), () => this.defaultWorldTechLevel = tl))
                .ToList();

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    public override string Summary(Scenario scen) => "";

    public override int GetHashCode() => base.GetHashCode() ^ this.defaultWorldTechLevel.GetHashCode();
}
