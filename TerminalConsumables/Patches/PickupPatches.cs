using GameData;
using HarmonyLib;
using LevelGeneration;
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

        [HarmonyPatch(typeof(Gear.ResourcePackPickup), nameof(Gear.ResourcePackPickup.OnSyncStateChange))]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static void FixDroppedPackArea(Gear.ResourcePackPickup __instance, ePickupItemStatus status, ref pPickupPlacement placement)
        {
            if (status == ePickupItemStatus.PlacedInLevel && placement.node.TryGet(out var courseNode))
            {
                __instance.m_terminalItem.SpawnNode = courseNode;
                __instance.m_terminalItem.FloorItemLocation = courseNode.m_zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore);
            }
        }
    }
}
