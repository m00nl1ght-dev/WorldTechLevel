using LunarFramework.GUI;
using LunarFramework.Utility;
using Verse;

namespace WorldTechLevel;

public class WorldTechLevelSettings : LunarModSettings
{
    public readonly Entry<bool> AlwaysAllowOffworld = MakeEntry(false);
    public readonly Entry<bool> AlwaysAllowNeurotrainers = MakeEntry(false);

    public readonly Entry<bool> Filter_Factions = MakeEntry(true);
    public readonly Entry<bool> Filter_Research = MakeEntry(true);
    public readonly Entry<bool> Filter_Items = MakeEntry(true);
    public readonly Entry<bool> Filter_Quests = MakeEntry(true);
    public readonly Entry<bool> Filter_Incidents = MakeEntry(true);
    public readonly Entry<bool> Filter_PawnKinds = MakeEntry(true);
    public readonly Entry<bool> Filter_Apparel = MakeEntry(true);
    public readonly Entry<bool> Filter_Weapons = MakeEntry(true);
    public readonly Entry<bool> Filter_Possessions = MakeEntry(true);
    public readonly Entry<bool> Filter_Prosthetics = MakeEntry(true);
    public readonly Entry<bool> Filter_Backstories = MakeEntry(true);
    public readonly Entry<bool> Filter_Traits = MakeEntry(true);
    public readonly Entry<bool> Filter_Diseases = MakeEntry(true);
    public readonly Entry<bool> Filter_Addictions = MakeEntry(true);
    public readonly Entry<bool> Filter_DamageTypes = MakeEntry(true);
    public readonly Entry<bool> Filter_Ideoligions = MakeEntry(true);
    public readonly Entry<bool> Filter_Xenotypes = MakeEntry(true);
    public readonly Entry<bool> Filter_BuildingMaterials = MakeEntry(true);
    public readonly Entry<bool> Filter_MineableResources = MakeEntry(true);
    public readonly Entry<bool> Filter_GenSteps = MakeEntry(true);
    public readonly Entry<bool> Filter_WorldGenSteps = MakeEntry(true);

    protected override string TranslationKeyPrefix => "WorldTechLevel.Settings";

    private bool changedLevels;
    private bool changedFilters;

    public WorldTechLevelSettings() : base(WorldTechLevel.LunarAPI)
    {
        MakeTab("Tab.General", DoGeneralSettingsTab);
        MakeTab("Tab.Filters", DoFiltersSettingsTab);
    }

    public void DoGeneralSettingsTab(LayoutRect layout)
    {
        layout.PushChanged();

        LunarGUI.Checkbox(layout, ref AlwaysAllowOffworld.Value, Label("AlwaysAllowOffworld"));
        LunarGUI.Checkbox(layout, ref AlwaysAllowNeurotrainers.Value, Label("AlwaysAllowNeurotrainers"));

        if (layout.PopChanged())
        {
            changedLevels = true;
        }
    }

    public void DoFiltersSettingsTab(LayoutRect layout)
    {
        layout.PushChanged();

        foreach (var (key, value) in Entries)
        {
            if (key.StartsWith("Filter") && value is Entry<bool> entry)
            {
                LunarGUI.Checkbox(layout, ref entry.Value, Label(key), Label($"{key}.Hint"));
            }
        }

        if (layout.PopChanged())
        {
            changedFilters = true;
        }
    }

    public void ApplyChangesIfDirty()
    {
        if (changedLevels)
        {
            EffectiveTechLevels.Initialize();
            changedLevels = false;
        }

        if (changedFilters)
        {
            WorldTechLevel.FiltersPatchGroup.ReApply();
            changedFilters = false;
        }
    }
}
