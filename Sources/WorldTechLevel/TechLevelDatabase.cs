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
    internal static TechLevel[] Data = [];

    internal static void Initialize(Func<T, TechLevel> func = null)
    {
        var defs = DefDatabase<T>.AllDefsListForReading;
        var data = new TechLevel[defs.Count];

        if (func != null)
            for (int i = 0; i < defs.Count; i++)
                data[i] = func(defs[i]);

        Data = data;
    }

    internal static void Apply(Func<T, TechLevel, TechLevel> func)
    {
        var defs = DefDatabase<T>.AllDefsListForReading;
        var updated = new TechLevel[defs.Count];

        for (int i = 0; i < defs.Count; i++)
            updated[i] = func(defs[i], Data[i]);

        Data = updated;
    }

    internal static void ApplyOverrides()
    {
        var overrides = DefDatabase<TechLevelOverrideDef>.AllDefs
            .Where(d => d.defType == typeof(T))
            .SelectMany(d => d.overrides)
            .Where(d => d.unlessModPresent == null || !ModsConfig.IsActive(d.unlessModPresent))
            .Where(d => d.ifModPresent == null || ModsConfig.IsActive(d.ifModPresent))
            .Where(d => !d.offworld || WorldTechLevel.Settings.AlwaysAllowOffworld)
            .OrderBy(e => e.priority);

        foreach (var entry in overrides)
            if (DefDatabase<T>.defsByName.TryGetValue(entry.defName, out var def) && def.index < Data.Length)
                if (entry.priority >= 0 || Data[def.index] == TechLevel.Undefined)
                    Data[def.index] = entry.techLevel;
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

        foreach (var group in defs.GroupBy(d => Data[d.index]).OrderByDescending(g => g.Key))
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
