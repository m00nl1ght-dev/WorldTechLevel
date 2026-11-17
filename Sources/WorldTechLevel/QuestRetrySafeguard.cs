using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorldTechLevel;

/// <summary>
/// If a mod or game component repeatedly re-attempts generating the same quest after it was blocked by WTL,
/// this safeguard excludes it from quest filtering in order to prevent lag and other potential issues.
/// </summary>
public static class QuestRetrySafeguard
{
    private static readonly Dictionary<string, Entry> _retryStats = [];

    private const int checkInterval = 90_000; // 1.5 in-game days
    private const int maxCountPerInterval = 5;

    public static int GetRetryCount(QuestScriptDef def)
    {
        if (_retryStats.TryGetValue(def.defName, out var entry))
        {
            if (Current.Game.tickManager.TicksGame - entry.lastTryTick < checkInterval)
            {
                return entry.count;
            }
        }

        return 0;
    }

    public static bool Check(QuestScriptDef def, bool increment)
    {
        var count = GetRetryCount(def);

        if (count >= maxCountPerInterval)
        {
            if (increment)
            {
                WorldTechLevel.Logger.Error(
                    $"A mod or game component has repeatedly re-attempted to generate the quest " +
                    $"{def.defName} (from {def.modContentPack}) after it was blocked by World Tech Level. " +
                    $"To prevent lag and other potential issues, this quest will now be allowed " +
                    $"to generate, even though it exceeds the world's tech level."
                );
            }

            return false;
        }

        if (increment)
        {
            _retryStats[def.defName] = new Entry(count + 1, Current.Game.tickManager.TicksGame);
        }

        return true;
    }

    private readonly struct Entry
    {
        public readonly int count;
        public readonly int lastTryTick;

        public Entry(int count, int lastTryTick)
        {
            this.count = count;
            this.lastTryTick = lastTryTick;
        }
    }
}
