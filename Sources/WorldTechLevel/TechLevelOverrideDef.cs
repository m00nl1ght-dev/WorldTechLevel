using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorldTechLevel;

public class TechLevelOverrideDef : Def
{
    public Type defType;
    public List<Entry> overrides;

    public struct Entry
    {
        public string defName;
        public TechLevel techLevel;
        public string unlessModPresent;
        public string ifModPresent;
        public int priority;
    }
}
