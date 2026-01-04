using GameData;
using HarmonyLib;
using TerminalConsumables.Managers;

namespace TerminalConsumables.Patches
{
    [HarmonyPatch]
    internal static class PickupPatches
    {
        [HarmonyPatch(typeof(ConsumablePickup_Core), nameof(ConsumablePickup_Core.Setup))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void PostSetup(ConsumablePickup_Core __instance, ItemDataBlock data)
        {
            if (!data.registerInTerminalSystem || string.IsNullOrEmpty(data.terminalItemShortName) || string.IsNullOrEmpty(data.terminalItemLongName)) return;

            TerminalItemManager.AddTerminalItem(__instance);
        }
    }
}
