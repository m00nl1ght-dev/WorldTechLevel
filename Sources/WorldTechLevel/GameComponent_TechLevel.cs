using RimWorld;
using Verse;

namespace WorldTechLevel;

public class GameComponent_TechLevel : GameComponent
{
    private TechLevel _worldTechLevel = TechLevel.Archotech;

    public GameComponent_TechLevel(Game game) { }

    public TechLevel WorldTechLevel
    {
        get => _worldTechLevel;
        set => _worldTechLevel = value;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref _worldTechLevel, "WorldTechLevel", TechLevel.Archotech);
    }
}
