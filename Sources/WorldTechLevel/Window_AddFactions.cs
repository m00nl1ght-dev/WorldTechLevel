using System;
using System.Collections.Generic;
using System.Linq;
using LunarFramework.GUI;
using LunarFramework.Utility;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace WorldTechLevel;

[HotSwappable]
public class Window_AddFactions : Window
{
    public override Vector2 InitialSize => new(500f, 450f);

    private readonly LayoutRect _windowLayout = new(WorldTechLevel.LunarAPI);
    private readonly LayoutRect _listingLayout = new(WorldTechLevel.LunarAPI);

    private readonly FactionDef[] _factions;
    private readonly bool[] _selected;

    private Vector2 _listingScrollPosition;
    private Rect _listingViewRect;

    public Window_AddFactions(List<FactionDef> factions)
    {
        this.absorbInputAroundWindow = true;
        this.closeOnAccept = false;
        this.closeOnCancel = false;
        this.forcePause = true;

        _factions = factions.ToArray();
        _selected = new bool[_factions.Length];
    }

    public static void OpenIfAnyAvailable(TechLevel prevTechLevel)
    {
        if (!WorldTechLevel.Settings.Filter_Factions) return;

        var factionManager = Find.FactionManager;

        var available = DefDatabase<FactionDef>.AllDefs
            .Where(d => d.displayInFactionSelection && !d.hidden && !d.isPlayer && d.maxConfigurableAtWorldCreation > 0)
            .Where(d => d.EffectiveTechLevel() > prevTechLevel && d.EffectiveTechLevel() <= WorldTechLevel.Current)
            .Where(d => factionManager.AllFactions.All(f => f.def != d))
            .ToList();

        if (available.Any())
            Find.WindowStack.Add(new Window_AddFactions(available));
    }

    public override void DoWindowContents(Rect rect)
    {
        _windowLayout.BeginRoot(rect);

        LunarGUI.Label(_windowLayout, "WorldTechLevel.AddFactions.Message".Translate());

        _windowLayout.Abs(10f);

        LunarGUI.BeginScrollView(_windowLayout.Abs(InitialSize.y - 110f), ref _listingViewRect, ref _listingScrollPosition);

        _listingLayout.BeginRoot(_listingViewRect, new LayoutParams { Spacing = 10f });

        for (var i = 0; i < _factions.Length; i++)
        {
            var faction = _factions[i];

            _listingLayout.BeginAbs(28f, new LayoutParams { Horizontal = true, Reversed = true, Spacing = 10f });

            GUI.color = Color.white.ToTransparent(i % 2 == 0 ? 0.4f : 0.2f);
            GUI.DrawTexture(_listingLayout, TexUI.HighlightTex);
            GUI.color = Color.white;

            TooltipHandler.TipRegion(_listingLayout, faction.Description);

            var checkRect = _listingLayout.Abs(24f);

            Widgets.CheckboxDraw(checkRect.x - 2f, checkRect.y + 2f, _selected[i], false, checkRect.width);

            if (Widgets.ButtonInvisible(_listingLayout))
                _selected[i] = !_selected[i];

            LunarGUI.Label(_listingLayout.Rel(-1).MoveBy(7f, 4f), faction.LabelCap);

            _listingLayout.End();
        }

        _listingViewRect.height = Mathf.Max(_listingViewRect.height, _listingLayout.OccupiedSpace);
        _listingLayout.End();

        LunarGUI.EndScrollView();

        _windowLayout.Abs(10f);

        if (LunarGUI.Button(_windowLayout, "Confirm".Translate()))
        {
            for (var i = 0; i < _factions.Length; i++)
            {
                try
                {
                    if (_selected[i]) FactionGenerator.CreateFactionAndAddToManager(_factions[i]);
                }
                catch (Exception e)
                {
                    WorldTechLevel.Logger.Error($"Error occured while generating faction of type {_factions[i]}", e);
                    Messages.Message("WorldTechLevel.AddFactions.Error".Translate(_factions[i].LabelCap), MessageTypeDefOf.RejectInput, false);
                }
            }

            Find.IdeoManager.SortIdeos();

            var added = Find.FactionManager.AllFactions.Where(f => _factions.Contains(f.def)).ToList();

            foreach (var faction in added)
            {
                for (int i = 0; i < Rand.RangeInclusive(3, 7); i++)
                {
                    var settlement = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.SetFaction(faction);
                    settlement.Tile = TileFinder.RandomSettlementTileFor(faction);
                    settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                    Find.WorldObjects.Add(settlement);
                }
            }

            if (added.Any())
                Messages.Message("WorldTechLevel.AddFactions.Success".Translate(added.Count), MessageTypeDefOf.NeutralEvent, false);

            Close();
        }

        _windowLayout.End();
    }
}
