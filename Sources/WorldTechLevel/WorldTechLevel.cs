using LunarFramework;
using LunarFramework.Logging;
using LunarFramework.Patching;
using RimWorld;
using Verse;

namespace WorldTechLevel;

[LunarComponentEntrypoint]
public class WorldTechLevel : Mod
{
    internal static readonly LunarAPI LunarAPI = LunarAPI.Create("WorldTechLevel", Init, Cleanup);

    internal static LogContext Logger => LunarAPI.LogContext;

    internal static PatchGroup MainPatchGroup;
    internal static PatchGroup CompatPatchGroup;

    internal static TechLevel Current = TechLevel.Archotech;

    private static void Init()
    {
        CompatPatchGroup ??= LunarAPI.RootPatchGroup.NewSubGroup("Compat");
        CompatPatchGroup.Subscribe();

        ModCompat.ApplyAll(LunarAPI, CompatPatchGroup);

        EffectiveTechLevels.Initialize();
    }

    private static void Cleanup()
    {
        CompatPatchGroup?.UnsubscribeAll();
    }

    public WorldTechLevel(ModContentPack content) : base(content)
    {
        MainPatchGroup ??= LunarAPI.RootPatchGroup.NewSubGroup("Main");
        MainPatchGroup.AddPatches(typeof(WorldTechLevel).Assembly);
        MainPatchGroup.Subscribe();
    }
}
