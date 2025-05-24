using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WorldTechLevel.Patches;

[PatchGroup("Filters")]
[HarmonyPatch(typeof(BaseGen))]
internal static class Patch_BaseGen
{
    [HarmonyPrepare]
    private static bool IsFilterEnabled() => WorldTechLevel.Settings.Filter_GenSteps;

    private static readonly Dictionary<FactionDef, TechLevel> _originalTechLevels = [];

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(BaseGen.Generate))]
    private static void Generate_Prefix()
    {
        foreach (var def in DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer))
        {
            _originalTechLevels[def] = def.techLevel;
            def.techLevel = TechLevelUtility.Min(def.techLevel, WorldTechLevel.Current);
        }
    }

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(BaseGen.Generate))]
    private static void Generate_Finalizer()
    {
        foreach (var def in DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer))
        {
            def.techLevel = _originalTechLevels[def];
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BaseGen.Resolve))]
    private static IEnumerable<CodeInstruction> Resolve_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var ldloc = new CodeInstruction(OpCodes.Ldloca_S);

        var pattern1 = TranspilerPattern.Build("ProcessParams")
            .MatchLoad(typeof(SymbolStack.Element), nameof(SymbolStack.Element.resolveParams)).Keep()
            .Insert(CodeInstruction.Call(typeof(Patch_BaseGen), nameof(ProcessParams)));

        var pattern2 = TranspilerPattern.Build("ProcessRules")
            .Match(OpCodes.Ldloca_S).StoreOperandIn(ldloc).Keep()
            .MatchCall(typeof(Dictionary<string, List<RuleDef>>), nameof(Dictionary<string, List<RuleDef>>.TryGetValue)).Keep()
            .Insert(ldloc).Insert(CodeInstruction.Call(typeof(Patch_BaseGen), nameof(ProcessRules)));

        return TranspilerPattern.Apply(instructions, pattern1, pattern2);
    }

    private static ResolveParams ProcessParams(ResolveParams resolveParams)
    {
        resolveParams.sleepingInsectsWakeupSignalTag ??= resolveParams.sleepingMechanoidsWakeupSignalTag;
        return resolveParams;
    }

    private static void ProcessRules(ref List<RuleDef> rulesRef)
    {
        if (rulesRef != null && rulesRef.Any(d => d.MinRequiredTechLevel() > WorldTechLevel.Current))
        {
            rulesRef = rulesRef.FilterWithAlternatives().ToList();
        }
    }
}
