using RimWorld;
using Verse;

namespace WorldTechLevel;

public class GameComponent_TechLevel : GameComponent
{
    public TechLevel WorldTechLevel = TechLevel.Archotech;

    public GameComponent_TechLevel(Game game) { }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref WorldTechLevel, "WorldTechLevel", TechLevel.Archotech);
    }
}
