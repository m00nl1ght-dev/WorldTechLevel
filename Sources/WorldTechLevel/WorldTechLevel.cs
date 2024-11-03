using LunarFramework;
using LunarFramework.Logging;
using LunarFramework.Patching;
using RimWorld;
using UnityEngine;
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

    internal static WorldTechLevelSettings Settings;

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
        Settings = GetSettings<WorldTechLevelSettings>();

        MainPatchGroup ??= LunarAPI.RootPatchGroup.NewSubGroup("Main");
        MainPatchGroup.AddPatches(typeof(WorldTechLevel).Assembly);
        MainPatchGroup.Subscribe();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        Settings.ApplyChangesIfDirty();
    }

    public override string SettingsCategory() => "World Tech Level";
}
