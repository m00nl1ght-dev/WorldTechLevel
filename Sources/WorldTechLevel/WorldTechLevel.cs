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
    internal static PatchGroup FiltersPatchGroup;
    internal static PatchGroup CompatPatchGroup;

    public static TechLevel Current { get; set; } = TechLevel.Archotech;

    internal static WorldTechLevelSettings Settings;

    private static void Init()
    {
        #if DEBUG
        Logger.Level = LogContext.LogLevel.Debug;
        #endif

        MainPatchGroup ??= LunarAPI.RootPatchGroup.NewSubGroup("Main");
        MainPatchGroup.AddPatches(typeof(WorldTechLevel).Assembly);
        MainPatchGroup.Subscribe();

        FiltersPatchGroup ??= LunarAPI.RootPatchGroup.NewSubGroup("Filters");
        FiltersPatchGroup.AddPatches(typeof(WorldTechLevel).Assembly);
        FiltersPatchGroup.Subscribe();

        CompatPatchGroup ??= LunarAPI.RootPatchGroup.NewSubGroup("Compat");
        CompatPatchGroup.Subscribe();

        ModCompat.ApplyAll(LunarAPI, CompatPatchGroup);

        MainPatchGroup.CheckForConflicts(Logger);
        FiltersPatchGroup.CheckForConflicts(Logger);

        DefTechLevels.Initialize();
        Settings.LogFilterInfo();
    }

    private static void Cleanup()
    {
        MainPatchGroup?.UnsubscribeAll();
        FiltersPatchGroup?.UnsubscribeAll();
        CompatPatchGroup?.UnsubscribeAll();
    }

    public WorldTechLevel(ModContentPack content) : base(content)
    {
        Settings = GetSettings<WorldTechLevelSettings>();
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
