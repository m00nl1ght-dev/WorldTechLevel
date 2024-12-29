using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using Verse;

// ReSharper disable StaticMemberInGenericType

namespace WorldTechLevel;

internal static class TechLevelDatabase<T> where T : Def
{
    internal static TechLevel[] Levels = [];
    internal static Alternative[][] Alternatives = [];

    internal static void Initialize(Func<T, TechLevel> func = null)
    {
        var defs = DefDatabase<T>.AllDefsListForReading;
        var levels = new TechLevel[defs.Count];

        if (func != null)
            for (int i = 0; i < defs.Count; i++)
                levels[i] = func(defs[i]);

        Levels = levels;

        var altConfigs = DefDatabase<TechLevelConfigDef>.AllDefs
            .Where(def => def.defType == typeof(T) && def.alternatives != null);

        if (altConfigs.Any())
        {
            var alternatives = new Alternative[defs.Count][];

            foreach (var altEntry in altConfigs.SelectMany(c => c.alternatives))
            {
                var options = altEntry.options
                    .Select(e => new Alternative(DefDatabase<T>.GetNamedSilentFail(e.defName), e.weight))
                    .Where(a => a is { def: not null, weight: > 0f })
                    .ToArray();

                foreach (var defName in altEntry.targets)
                {
                    void Process(T value)
                    {
                        var existing = alternatives[value.index];
                        alternatives[value.index] = existing != null ? existing.Concat(options).Distinct().ToArray() : options;
                    }

                    if (defName.EndsWith("*"))
                    {
                        var prefix = defName.Substring(0, defName.Length - 1);

                        foreach (var def in DefDatabase<T>.AllDefs.Where(d => d.defName.StartsWith(prefix)))
                            Process(def);
                    }
                    else if (DefDatabase<T>.defsByName.TryGetValue(defName, out var def))
                    {
                        Process(def);
                    }
                }
            }

            Alternatives = alternatives;
        }
    }

    internal static void EnsureInitialized()
    {
        if (DefDatabase<T>.defsList.Count != Levels.Length && Levels.Length > 0)
        {
            WorldTechLevel.Logger.Warn($"{typeof(T).Name} database was modified, re-initializing tech levels.");
            EffectiveTechLevels.Initialize();
        }
    }

    internal static void Apply(Func<T, TechLevel, TechLevel> func)
    {
        var defs = DefDatabase<T>.AllDefsListForReading;
        var updated = new TechLevel[defs.Count];

        for (int i = 0; i < defs.Count; i++)
            updated[i] = func(defs[i], Levels[i]);

        Levels = updated;
    }

    internal static void ApplyOverrides()
    {
        var overrides = DefDatabase<TechLevelConfigDef>.AllDefs
            .Where(d => d.defType == typeof(T) && d.entries != null)
            .SelectMany(d => d.entries)
            .Where(d => d.unlessModPresent == null || !ModsConfig.IsActive(d.unlessModPresent))
            .Where(d => d.ifModPresent == null || ModsConfig.IsActive(d.ifModPresent))
            .Where(d => !d.offworld || !WorldTechLevel.Settings.AlwaysAllowOffworld)
            .OrderBy(e => e.priority);

        foreach (var entry in overrides)
        {
            void Process(T def)
            {
                if (entry.contentPack == null || def.modContentPack?.PackageId == entry.contentPack)
                    if (entry.priority >= 0 || Levels[def.index] == TechLevel.Undefined)
                        Levels[def.index] = entry.techLevel;
            }

            if (entry.defName == null)
            {
                foreach (var def in DefDatabase<T>.AllDefs)
                    Process(def);
            }
            else if (entry.defName.EndsWith("*"))
            {
                var prefix = entry.defName.Substring(0, entry.defName.Length - 1);

                foreach (var def in DefDatabase<T>.AllDefs.Where(d => d.defName.StartsWith(prefix)))
                    Process(def);
            }
            else if (DefDatabase<T>.defsByName.TryGetValue(entry.defName, out var def))
            {
                Process(def);
            }
        }
    }

    public readonly struct Alternative
    {
        public readonly T def;
        public readonly float weight;

        public Alternative(T def, float weight)
        {
            this.def = def;
            this.weight = weight;
        }
    }

    private static readonly List<string> DebugExcludedPrefixes = [
        "Mote_", "Bullet_", "Filth_", "SignalAction_", "Blueprint_", "Frame_", "Meat_", "Leather_", "Corpse_"
    ];

    internal static void DebugOutput()
    {
        var defs = DefDatabase<T>.AllDefsListForReading;

        var folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var file = Path.Combine(folder, $"{typeof(T).Name}.log");
        var lines = new List<string>();

        foreach (var group in defs.GroupBy(d => Levels[d.index]).OrderByDescending(g => g.Key))
        {
            lines.Add("");
            lines.Add($"### {group.Key.ToString()} ###");
            lines.Add("");

            lines.AddRange(group
                .Where(def => !DebugExcludedPrefixes.Any(p => def.defName.StartsWith(p)))
                .Select(def => def.defName));
        }

        if (File.Exists(file)) File.Delete(file);
        File.WriteAllLines(file, lines);
    }
}
