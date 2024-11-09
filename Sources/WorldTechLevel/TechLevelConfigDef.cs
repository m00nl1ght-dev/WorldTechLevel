using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public class TechLevelConfigDef : Def
{
    public Type defType;
    public List<LevelEntry> entries;
    public List<AlternativesEntry> alternatives;

    public struct LevelEntry
    {
        public string defName;
        public TechLevel techLevel;
        public string unlessModPresent;
        public string ifModPresent;
        public bool offworld;
        public int priority;
    }

    public struct AlternativesEntry
    {
        public List<string> targets;
        public List<AlternativesGroupEntry> options;
    }

    public struct AlternativesGroupEntry
    {
        public string defName;
        public float weight;
    }
}
