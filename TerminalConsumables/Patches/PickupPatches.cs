using GameData;
using HarmonyLib;
using LevelGeneration;
using TerminalConsumables.Utils;
using System.Collections.Generic;

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

            string key = data.terminalItemShortName;
            if (data.addSerialNumberToName)
                key += "_"  + SerialGenerator.GetUniqueSerialNo();

            var terminalItem = __instance.gameObject.AddComponent<LG_GenericTerminalItem>();
            terminalItem.FloorItemType = eFloorInventoryObjectType.Resources;
            terminalItem.FloorItemStatus = eFloorInventoryObjectStatus.Normal;
            terminalItem.OverrideCode = "FIXQ";

            terminalItem.Setup(key);
            PickupManager.RegisterTerminalInfo(__instance, terminalItem, key, (List<string> defaultDetails) =>
            {
                List<string> list = new()
                {
                    "----------------------------------------------------------------",
                    "CONSUMABLE - " + data.terminalItemLongName.ToString(),
                    "CAPACITY: " + (__instance.pItemData.custom.ammo / data.ConsumableAmmoMax).ToString("P0")
                };
                list.AddRange(defaultDetails);
                list.Add("----------------------------------------------------------------");
                return list;
            });
        }

        [HarmonyPatch(typeof(ConsumablePickup_Core), nameof(ConsumablePickup_Core.OnSyncStateChange))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void PostStateChange(ConsumablePickup_Core __instance, ePickupItemStatus status, ref pPickupPlacement placement)
        {
            if (!PickupManager.TryGetTerminalInfo(__instance, out var comps)) return;

            (_, var terminalItem, string itemKey) = comps;
            switch (status)
            {
                case ePickupItemStatus.PlacedInLevel:
                    if (placement.node.TryGet(out var courseNode))
                    {
                        terminalItem.SpawnNode = courseNode;
                        terminalItem.FloorItemLocation = courseNode.m_zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore);
                        LG_LevelInteractionManager.RegisterTerminalItem(terminalItem, itemKey);
                    }
                    break;
                case ePickupItemStatus.PickedUp:
                    LG_LevelInteractionManager.DeregisterTerminalItem(terminalItem);
                    break;
            }
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

        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.Query))]
        [HarmonyPrefix]
        static bool Prefix(LG_ComputerTerminalCommandInterpreter __instance, string param1)
        {
            if (!LG_LevelInteractionManager.TryGetTerminalInterface(param1.ToUpper(), __instance.m_terminal.SpawnNode.m_dimension.DimensionIndex, out var target)) return true;

            if (!PickupManager.HasTerminal(target)) return true;

            __instance.AddOutput(TerminalLineType.SpinningWaitDone, "Querying " + param1.ToUpper(), 3f);
            string pingStatus = __instance.GetPingStatus(target);

            var details = __instance.GetDefaultDetails(target, pingStatus);
            List<string> detailsManaged = new(details.Count);
            foreach (string line in details)
                detailsManaged.Add(line);

            if (!PickupManager.TryGetQueryText(target, detailsManaged, out var output))
            {
                DinoLogger.Error("Unable to get query text!");
                return false;
            }

            __instance.AddOutputEmptyLine();
            foreach (var line in output)
            {
                __instance.AddOutput(line, spacing: false);
            }
            __instance.AddOutputEmptyLine();
            return false;
        }
    }
}
