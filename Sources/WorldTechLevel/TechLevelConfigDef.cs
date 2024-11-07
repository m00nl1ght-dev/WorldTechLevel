using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public class TechLevelConfigDef : Def
{
    public Type defType;
    public List<Entry> entries;

    public struct Entry
    {
        public string defName;
        public TechLevel techLevel;
        public string unlessModPresent;
        public string ifModPresent;
        public bool offworld;
        public int priority;
    }
}
