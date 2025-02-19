using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace TerminalConsumables
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.0.0")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "TerminalConsumables";

        public override void Load()
        {
            new Harmony(MODNAME).PatchAll();
            GTFO.API.LevelAPI.OnLevelCleanup += PickupManager.OnLevelCleanup;
            Log.LogMessage("Loaded " + MODNAME);
        }
    }
}