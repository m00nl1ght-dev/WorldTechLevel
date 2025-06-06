using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LunarFramework.GUI;
using LunarFramework.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorldTechLevel;

public class WorldTechLevelSettings : LunarModSettings
{
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
    public readonly Entry<bool> Filter_UserInterface = MakeEntry(true);

    public readonly Entry<Dictionary<string, TechLevel>> Overrides = MakeEntry(new Dictionary<string, TechLevel>());

    public readonly Entry<bool> AlwaysAllowOffworld = MakeEntry(false);
    public readonly Entry<bool> AlwaysDefaultToUnrestricted = MakeEntry(false);

    public readonly Entry<List<string>> FactionsExcluded = MakeEntry(new List<string>());

    protected override string TranslationKeyPrefix => "WorldTechLevel.Settings";

    private List<IDefListing> _listings;
    private IDefListing _currentListing;
    private List<ModContentPack> _contentPacks;
    private ModContentPack _currentContentPack;
    private List<Def> _currentDefs;
    private string _searchString = "";
    private bool _showOnlyNonDefault;
    private bool _sortByTechLevel;
    private bool _anyDefLabels;

    private readonly LayoutRect _listingLayout;
    private Vector2 _listingScrollPosition;
    private Rect _listingViewRect;

    private bool _changedLevels;
    private bool _changedFilters;

    public WorldTechLevelSettings() : base(WorldTechLevel.LunarAPI)
    {
        MakeTab("Tab.Filters", DoFiltersSettingsTab);
        MakeTab("Tab.Overrides", DoOverridesSettingsTab);
        MakeTab("Tab.Misc", DoMiscSettingsTab);

        _listingLayout = new LayoutRect(WorldTechLevel.LunarAPI);
    }

    public void DoFiltersSettingsTab(LayoutRect layout)
    {
        var entryIdx = 0;

        foreach (var (key, value) in Entries)
        {
            if (key.StartsWith("Filter") && value is Entry<bool> entry)
            {
                if (key == "Filter_Ideoligions" && !ModsConfig.IdeologyActive) continue;
                if (key == "Filter_Xenotypes" && !ModsConfig.BiotechActive) continue;

                layout.BeginAbs(28f, new LayoutParams { Horizontal = true, Reversed = true, Spacing = 10f });

                GUI.color = Color.white.ToTransparent(entryIdx % 2 == 0 ? 0.4f : 0.2f);
                GUI.DrawTexture(layout, TexUI.HighlightTex);
                GUI.color = Color.white;

                TooltipHandler.TipRegion(layout, Label($"{key}.Hint"));

                var checkRect = layout.Abs(24f);

                Widgets.CheckboxDraw(checkRect.x - 2f, checkRect.y + 2f, entry.Value, false, checkRect.width);

                if (Widgets.ButtonInvisible(layout))
                {
                    entry.Value = !entry.Value;
                    _changedFilters = true;
                }

                LunarGUI.Label(layout.Rel(-1).MoveBy(7f, 4f), Label(key));

                layout.End();

                entryIdx++;
            }
        }
    }

    public void DoOverridesSettingsTab(LayoutRect layout)
    {
        if (_listings == null) SetupListings();
        if (_currentDefs == null) UpdateListing();

        layout.BeginAbs(28f, new LayoutParams { Horizontal = true, Spacing = 10f });

        LunarGUI.Dropdown(layout.Abs(200f), _currentListing, _listings, SelectListing, d => d.Label);

        LunarGUI.Dropdown(layout.Abs(200f), _currentContentPack, _contentPacks, SelectMcp,
            d => d != null ? d.Name : Label("DefListing.AnyContentSource"));

        if (LunarGUI.Button(layout.Abs(30f), "~"))
        {
            var options = new List<FloatMenuOption>();

            foreach (var value in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
            {
                if (value != TechLevel.Animal)
                {
                    options.Add(new FloatMenuOption("WorldTechLevel.Settings.DefListing.SetAllInList".Translate(value.RevSelectionLabel()), () =>
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(Label("DefListing.ConfirmSetAllInList"), SetAllInList))));

                    void SetAllInList()
                    {
                        foreach (var def in _currentDefs)
                        {
                            _currentListing.SetLevelFor(def, value);
                            Overrides.Value[KeyFor(def)] = value;
                        }
                    }
                }
            }

            options.Add(new FloatMenuOption(Label("DefListing.ResetAllInList"), () =>
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(Label("DefListing.ConfirmResetAllInList"), ResetAllInList))));

            options.Add(new FloatMenuOption(Label(_showOnlyNonDefault ? "DefListing.ShowAll" : "DefListing.ShowOnlyNonDefault"), () =>
            {
                _showOnlyNonDefault = !_showOnlyNonDefault;
                UpdateListing();
            }));

            options.Add(new FloatMenuOption(Label(_sortByTechLevel ? "DefListing.SortByName" : "DefListing.SortByTechLevel"), () =>
            {
                _sortByTechLevel = !_sortByTechLevel;
                if (_sortByTechLevel && _changedLevels) ApplyLevels();
                UpdateListing();
            }));

            void ResetAllInList()
            {
                foreach (var def in _currentDefs)
                    Overrides.Value.Remove(KeyFor(def));

                ApplyLevels();
                UpdateListing();
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        layout.PushChanged();

        LunarGUI.TextField(layout.Rel(-1), ref _searchString);

        if (layout.PopChanged())
        {
            _searchString = _searchString.ToLower();
            UpdateListing();
        }

        layout.End();

        LunarGUI.BeginScrollView(layout.Abs(450f), ref _listingViewRect, ref _listingScrollPosition);

        _listingLayout.BeginRoot(_listingViewRect, new LayoutParams { Spacing = 10f });

        for (var i = 0; i < _currentDefs.Count; i++)
        {
            var def = _currentDefs[i];
            var techLevel = _currentListing.GetLevelFor(def);

            _listingLayout.BeginAbs(28f, new LayoutParams { Horizontal = true, Spacing = 10f });

            GUI.color = Color.white.ToTransparent(i % 2 == 0 ? 0.4f : 0.2f);
            GUI.DrawTexture(_listingLayout, TexUI.HighlightTex);
            GUI.color = Color.white;

            if (_anyDefLabels)
            {
                LunarGUI.Label(_listingLayout.Abs(300f).MoveBy(7f, 4f), def.defName);
                LunarGUI.Label(_listingLayout.Abs(300f).MoveBy(7f, 4f), def.label);
            }
            else
            {
                LunarGUI.Label(_listingLayout.Abs(610f).MoveBy(7f, 4f), def.defName);
            }

            LunarGUI.Label(_listingLayout.Abs(300f).MoveBy(7f, 4f), techLevel == TechLevel.Undefined
                ? "WorldTechLevel.Unrestricted".Translate().CapitalizeFirst()
                : techLevel.ToStringHuman().CapitalizeFirst());

            if (Widgets.ButtonInvisible(_listingLayout))
            {
                if (Input.GetKey(KeyCode.LeftShift) && Current.ProgramState == ProgramState.Playing)
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(def));
                }
                else
                {
                    var options = new List<FloatMenuOption>();

                    foreach (var value in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
                    {
                        if (value != TechLevel.Animal)
                        {
                            options.Add(new FloatMenuOption(Label($"TechLevelOption.{value}"), SetOverride));

                            void SetOverride()
                            {
                                _currentListing.SetLevelFor(def, value);
                                Overrides.Value[KeyFor(def)] = value;
                            }
                        }
                    }

                    if (Overrides.Value.ContainsKey(KeyFor(def)))
                    {
                        options.Add(new FloatMenuOption(Label("TechLevelOption.Reset"), ResetOverride));

                        void ResetOverride()
                        {
                            Overrides.Value.Remove(KeyFor(def));
                            ApplyLevels();
                        }
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }

            _listingLayout.End();
        }

        _listingViewRect.height = Mathf.Max(_listingViewRect.height, _listingLayout.OccupiedSpace);
        _listingLayout.End();

        LunarGUI.EndScrollView();
    }

    public void DoMiscSettingsTab(LayoutRect layout)
    {
        layout.PushChanged();

        LunarGUI.Checkbox(layout, ref AlwaysAllowOffworld.Value, Label("AlwaysAllowOffworld"));

        if (layout.PopChanged())
        {
            _changedLevels = true;
        }

        LunarGUI.Checkbox(layout, ref AlwaysDefaultToUnrestricted.Value, Label("AlwaysDefaultToUnrestricted"));

        layout.BeginAbs(Text.LineHeight, new() { Reversed = true, Horizontal = true });

        if (LunarGUI.Button(layout.Abs(100), "WorldTechLevel.Settings.FactionsExcluded.Select".Translate()))
        {
            LunarGUI.OpenGenericWindow(WorldTechLevel.LunarAPI, new(500, 400), (_, layout) =>
            {
                layout.PushChanged();

                foreach (var def in DefDatabase<FactionDef>.AllDefsListForReading)
                {
                    if (!def.isPlayer)
                    {
                        LunarGUI.ToggleTableRow(layout, def.defName, false, def.LabelCap, FactionsExcluded.Value);
                    }
                }

                if (layout.PopChanged())
                {
                    _changedLevels = true;
                    _currentDefs = null;
                }
            });
        }

        LunarGUI.Label(layout.Abs(-1), "WorldTechLevel.Settings.FactionsExcluded".Translate(FactionsExcluded.Value.Count));

        layout.End();
    }

    private readonly string[] _excludedThingCategories = [
        "Animals", "Corpses", "Chunks", "Plants"
    ];

    private readonly string[] _excludedWorldGenSteps = [
        "Components", "Factions", "Terrain"
    ];

    private readonly string[] _excludedMapGenSteps = [
        "Terrain", "Fog"
    ];

    private void SetupListings()
    {
        _listings = [];

        foreach (var categoryDef in ThingCategoryDefOf.Root.childCategories)
            if (!_excludedThingCategories.Contains(categoryDef.defName))
                _listings.Add(new ThingDefCategoryListing(categoryDef));

        _listings.Add(new DefListing<TerrainDef>(d => d.BuildableByPlayer));
        _listings.Add(new DefListing<ResearchProjectDef>());
        _listings.Add(new DefListing<IncidentDef>());
        _listings.Add(new DefListing<QuestScriptDef>());
        _listings.Add(new DefListing<SitePartDef>());
        _listings.Add(new DefListing<GenStepDef>(d => !_excludedMapGenSteps.Contains(d.defName)));
        _listings.Add(new DefListing<WorldGenStepDef>(d => !_excludedWorldGenSteps.Contains(d.defName)));
        _listings.Add(new DefListing<TraitDef>());
        _listings.Add(new DefListing<PawnKindDef>());
        _listings.Add(new DefListing<BackstoryDef>());
        _listings.Add(new DefListing<FactionDef>(d =>
            d.maxConfigurableAtWorldCreation > 0 && d.displayInFactionSelection &&
            !FactionsExcluded.Value.Contains(d.defName))
        );

        if (ModsConfig.IdeologyActive)
        {
            _listings.Add(new DefListing<IdeoPresetDef>());
            _listings.Add(new DefListing<MemeDef>());
            _listings.Add(new DefListing<PreceptDef>());
            _listings.Add(new DefListing<RitualAttachableOutcomeEffectDef>());
        }

        if (ModsConfig.BiotechActive)
        {
            _listings.Add(new DefListing<XenotypeDef>(d => d != XenotypeDefOf.Baseliner));
        }

        SelectListing(_listings.First());
    }

    private void SelectListing(IDefListing listing)
    {
        _currentListing = listing;

        _contentPacks = LoadedModManager.RunningMods
            .Where(mcp => mcp.defs.Any(d => _currentListing.CanList(d)))
            .Prepend(null).ToList();

        UpdateListing();
    }

    private void SelectMcp(ModContentPack mcp)
    {
        _currentContentPack = mcp;
        UpdateListing();
    }

    private void UpdateListing()
    {
        var enumerable = _currentListing.BuildDefList();

        if (_currentContentPack != null) enumerable = enumerable
            .Where(d => d.modContentPack == _currentContentPack);

        if (_searchString.Length > 0) enumerable = enumerable
            .Where(d => d.defName.ToLower().Contains(_searchString) || (d.label != null && d.label.ToLower().Contains(_searchString)));

        if (_showOnlyNonDefault) enumerable = enumerable
            .Where(d => Overrides.Value.ContainsKey(KeyFor(d)));

        if (_sortByTechLevel)
            enumerable = enumerable.OrderByDescending(d => _currentListing.GetLevelFor(d)).ThenBy(d => d.defName);
        else
            enumerable = enumerable.OrderBy(d => d.defName);

        _currentDefs = enumerable.ToList();
        _anyDefLabels = _currentDefs.Any(d => d.label is { Length: > 0 });
        _listingScrollPosition = Vector2.zero;
        _listingViewRect.height = 450f;
    }

    public void ApplyChangesIfDirty()
    {
        if (_changedLevels)
        {
            ApplyLevels();
        }

        if (_changedFilters)
        {
            ApplyFilters();
        }

        _currentDefs = null;
    }

    private void ApplyLevels()
    {
        DefTechLevels.Initialize();
        RefreshResearchViewWidth();
        _changedLevels = false;
    }

    private void ApplyFilters()
    {
        WorldTechLevel.FiltersPatchGroup.ReApply();
        WorldTechLevel.CompatPatchGroup.ReApply();
        RefreshResearchViewWidth();
        _changedFilters = false;
        LogFilterInfo();
    }

    public void LogFilterInfo()
    {
        var str = Entries
            .Where(e => e.Key.StartsWith("Filter_") && e.Value is Entry<bool> { Value: false })
            .Select(e => e.Key.Substring(7))
            .Join();

        WorldTechLevel.Logger.Log(str.Length > 0 ? $"Filters initialized. Disabled filters: {str}" : "Filters initialized.");
    }

    public static void RefreshResearchViewWidth()
    {
        if (Current.Game != null && MainButtonDefOf.Research.TabWindow is MainTabWindow_Research researchTab)
        {
            researchTab.cachedVisibleResearchProjects = null;
            researchTab.rightViewWidth = researchTab.ViewSize(researchTab.CurTab).x;
        }
    }

    private static string KeyFor(Def def)
    {
        return $"{def.GetType().Name}:{def.defName}";
    }

    public interface IDefListing
    {
        public string Label { get; }

        public IEnumerable<Def> BuildDefList();

        public TechLevel GetLevelFor(Def def);

        public void SetLevelFor(Def def, TechLevel techLevel);

        public bool CanList(Def def);
    }

    public class DefListing<T> : IDefListing where T : Def
    {
        public string Label => $"WorldTechLevel.Settings.DefListing.{typeof(T).Name}".Translate().CapitalizeFirst();

        private readonly Predicate<T> _filter;

        public DefListing(Predicate<T> filter = null)
        {
            _filter = filter;
        }

        public IEnumerable<Def> BuildDefList()
        {
            TechLevelDatabase<T>.EnsureInitialized();
            return _filter == null ? DefDatabase<T>.AllDefs : DefDatabase<T>.AllDefs.Where(d => _filter(d));
        }

        public TechLevel GetLevelFor(Def def)
        {
            return TechLevelDatabase<T>.Levels[def.index];
        }

        public void SetLevelFor(Def def, TechLevel techLevel)
        {
            TechLevelDatabase<T>.Levels[def.index] = techLevel;
        }

        public bool CanList(Def def)
        {
            return def is T tDef && (_filter == null || _filter(tDef));
        }
    }

    public class ThingDefCategoryListing : IDefListing
    {
        public readonly ThingCategoryDef Category;

        private readonly string[] ExcludedPrefixes = ["Unfinished", "Egg", "Meat_", "Leather_"];

        public ThingDefCategoryListing(ThingCategoryDef category)
        {
            Category = category;
        }

        public string Label => Category.label.CapitalizeFirst();

        public IEnumerable<Def> BuildDefList()
        {
            TechLevelDatabase<ThingDef>.EnsureInitialized();
            return Category.DescendantThingDefs.Where(d => ExcludedPrefixes.All(p => !d.defName.StartsWith(p)));
        }

        public TechLevel GetLevelFor(Def def)
        {
            return TechLevelDatabase<ThingDef>.Levels[def.index];
        }

        public void SetLevelFor(Def def, TechLevel techLevel)
        {
            TechLevelDatabase<ThingDef>.Levels[def.index] = techLevel;
        }

        public bool CanList(Def def)
        {
            return def is ThingDef thingDef && Category.ContainedInThisOrDescendant(thingDef);
        }
    }
}
