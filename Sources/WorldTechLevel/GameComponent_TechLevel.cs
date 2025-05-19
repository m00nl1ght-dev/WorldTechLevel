using RimWorld;
using Verse;

namespace WorldTechLevel;

public class GameComponent_TechLevel : GameComponent
{
    private TechLevel _level = TechLevel.Archotech;

    public GameComponent_TechLevel(Game game) { }

    public TechLevel WorldTechLevel => _level;

    public void ChangeTo(TechLevel newLevel)
    {
        _level = newLevel;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref _level, "WorldTechLevel", TechLevel.Archotech);
    }
}
