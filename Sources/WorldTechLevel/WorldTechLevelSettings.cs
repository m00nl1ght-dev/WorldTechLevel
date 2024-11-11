using LunarFramework.GUI;
using LunarFramework.Utility;

namespace WorldTechLevel;

public class WorldTechLevelSettings : LunarModSettings
{
    public readonly Entry<bool> AlwaysAllowNeurotrainers = MakeEntry(false);
    public readonly Entry<bool> AlwaysAllowOffworld = MakeEntry(false);
    public readonly Entry<bool> FilterPawnEquipment = MakeEntry(false);
    public readonly Entry<bool> FilterPawnBackstories = MakeEntry(true);
    public readonly Entry<bool> FilterBuildingMaterials = MakeEntry(true);

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
        LunarGUI.Checkbox(layout, ref AlwaysAllowOffworld.Value, Label("AlwaysAllowOffworld"));
        LunarGUI.Checkbox(layout, ref FilterPawnEquipment.Value, Label("FilterPawnEquipment"));
        LunarGUI.Checkbox(layout, ref FilterPawnBackstories.Value, Label("FilterPawnBackstories"));
        LunarGUI.Checkbox(layout, ref FilterBuildingMaterials.Value, Label("FilterBuildingMaterials"));

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
