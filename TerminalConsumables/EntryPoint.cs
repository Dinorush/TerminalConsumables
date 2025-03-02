using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TerminalConsumables.Managers;

namespace TerminalConsumables
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.1.1")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "TerminalConsumables";

        public override void Load()
        {
            new Harmony(MODNAME).PatchAll();
            GTFO.API.LevelAPI.OnLevelCleanup += TerminalItemManager.OnLevelCleanup;
            Log.LogMessage("Loaded " + MODNAME);
        }
    }
}