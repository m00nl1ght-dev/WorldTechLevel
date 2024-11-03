using LunarFramework.GUI;
using LunarFramework.Utility;

namespace WorldTechLevel;

public class WorldTechLevelSettings : LunarModSettings
{
    public readonly Entry<bool> AlwaysAllowNeurotrainers = MakeEntry(false);

    protected override string TranslationKeyPrefix => "WorldTechLevel.Settings";

    private bool dirty;

    public WorldTechLevelSettings() : base(WorldTechLevel.LunarAPI)
    {
        MakeTab("Tab.General", DoGeneralSettingsTab);
    }

    public void DoGeneralSettingsTab(LayoutRect layout)
    {
        layout.PushChanged();

        LunarGUI.Checkbox(layout, ref AlwaysAllowNeurotrainers.Value, Label("AlwaysAllowNeurotrainers"));

        if (layout.PopChanged())
        {
            dirty = true;
        }
    }

    public void ApplyChangesIfDirty()
    {
        if (dirty)
        {
            EffectiveTechLevels.Initialize();
            dirty = false;
        }
    }
}
