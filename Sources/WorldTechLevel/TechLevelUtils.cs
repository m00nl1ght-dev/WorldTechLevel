using Verse;

namespace WorldTechLevel;

public static class TechLevelUtils
{
    public static GameComponent_TechLevel TechLevel(this Game game) =>
        game.GetComponent<GameComponent_TechLevel>();
}
