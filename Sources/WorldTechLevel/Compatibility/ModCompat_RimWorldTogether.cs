using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LunarFramework.Patching;
using RimWorld;
using RimWorld.Planet;
using Verse;
using WorldTechLevel.Patches;

namespace WorldTechLevel.Compatibility;

[HarmonyPatch]
internal class ModCompat_RimWorldTogether : ModCompat
{
    public override string TargetAssemblyName => "RTClient";
    public override string DisplayName => "RimWorld Together";

    internal static bool Active { get; private set; }

    private static MethodInfo _networkStateGetter;
    private static object _connectedState;

    protected override bool OnApply()
    {
        var sessionManager = AccessTools.GetTypesFromAssembly(TargetAssembly)
            .FirstOrDefault(t => t.Name == "SessionManager" && AccessTools.PropertyGetter(t, "CurrentNetworkState") != null);

        if (sessionManager == null) return false;

        var getter = AccessTools.PropertyGetter(sessionManager, "CurrentNetworkState");
        if (!getter.ReturnType.IsEnum || !Enum.IsDefined(getter.ReturnType, "Connected")) return false;

        _networkStateGetter = getter;
        _connectedState = Enum.Parse(getter.ReturnType, "Connected");

        Active = true;
        return true;
    }

    private static bool IsConnected => Equals(_networkStateGetter.Invoke(null, null), _connectedState);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.ResetFactionCounts))]
    private static void Page_CreateWorldParams_ResetFactionCounts_Postfix(Page_CreateWorldParams __instance)
    {
        var level = WorldTechLevel.Settings.Multiplayer_WorldTechLevel.Value;

        if (level != TechLevel.Undefined && IsConnected)
        {
            WorldTechLevel.Current = level;
            Patch_Page_CreateWorldParams.ApplyChanges(__instance.factions, ref __instance.pollution);
        }
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateWorld))]
    private static void WorldGenerator_GenerateWorld_Prefix()
    {
        var level = WorldTechLevel.Settings.Multiplayer_WorldTechLevel.Value;

        if (level != TechLevel.Undefined && IsConnected)
        {
            WorldTechLevel.Current = level;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Patch_WITab_Planet), nameof(Patch_WITab_Planet.FillTab_Postfix))]
    private static bool WITab_Planet_FillTab_Postfix_Prefix()
    {
        return !WorldTechLevel.Settings.Multiplayer_LockInGame || Prefs.DevMode || !IsConnected;
    }
}
