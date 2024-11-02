using RimWorld;
using Verse;

namespace WorldTechLevel;

public static class DefFilteringEngine
{
    private static TechLevel CurrentLevel = TechLevel.Archotech;

    public static void ApplyTechLevel(TechLevel techLevel)
    {
        if (CurrentLevel == techLevel) return;

        Log.Message($"Applying tech level change from {CurrentLevel.ToStringHuman()} to {techLevel.ToStringHuman()}");

        // TODO

        CurrentLevel = techLevel;
    }
}
