using LevelGeneration;
using System;
using System.Collections.Generic;
using TerminalConsumables.API;

namespace TerminalConsumables.Managers
{
    internal static class TerminalItemManager
    {
        private static readonly Dictionary<IntPtr, QueryInfo> _terminalInfo = new();

        public struct QueryInfo
        {
            public ItemInLevel item;
            public bool ammoRel;
            public QueryTextOverride? queryTextOverride;
        }

        public static bool ModifyTerminalItem(iTerminalItem? terminalItem, bool ammoRel = true, QueryTextOverride? queryOverride = null)
        {
            if (terminalItem == null) return false;
            
            if (!_terminalInfo.TryGetValue(terminalItem.Pointer, out QueryInfo queryInfo))
            {
                queryInfo.item = terminalItem.Cast<LG_GenericTerminalItem>().GetComponent<ItemInLevel>();
                if (queryInfo.item == null) return false;
            }

            queryInfo.ammoRel = ammoRel;
            queryInfo.queryTextOverride = queryOverride;
            _terminalInfo[terminalItem.Pointer] = queryInfo;
            return true;
        }

        public static iTerminalItem AddTerminalItem(ItemInLevel item, bool ammoRel = true, QueryTextOverride? queryOverride = null)
        {
            var terminalItem = item.GetComponent<iTerminalItem>();
            if (terminalItem != null)
            {
                ModifyTerminalItem(terminalItem, ammoRel, queryOverride);
                return terminalItem;
            }

            var data = item.ItemDataBlock;
            string itemKey = data.terminalItemShortName;
            if (data.addSerialNumberToName)
                itemKey += "_" + SerialGenerator.GetUniqueSerialNo();

            terminalItem = item.gameObject.AddComponent<LG_GenericTerminalItem>().Cast<iTerminalItem>();
            terminalItem.FloorItemType = eFloorInventoryObjectType.Resources;
            terminalItem.FloorItemStatus = eFloorInventoryObjectStatus.Normal;

            terminalItem.Setup(itemKey);
            item.internalSync.add_OnSyncStateChange((Action<ePickupItemStatus, pPickupPlacement, Player.PlayerAgent, bool>)(
                (ePickupItemStatus status, pPickupPlacement placement, Player.PlayerAgent player, bool isRecall) =>
                {
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
                }));

            _terminalInfo.Add(terminalItem.Pointer, new QueryInfo { item = item, ammoRel = ammoRel, queryTextOverride = queryOverride });

            return terminalItem;
        }

        public static void DoQueryOutput(iTerminalItem terminalItem, LG_ComputerTerminalCommandInterpreter interpreter)
        {
            if (!_terminalInfo.TryGetValue(terminalItem.Pointer, out var queryInfo)) return;

            var item = queryInfo.item;

            string pingStatus = interpreter.GetPingStatus(terminalItem);
            var details = interpreter.GetDefaultDetails(terminalItem, pingStatus);
            List<string> defaultDetails = new(details.Count);
            foreach (string line in details)
                defaultDetails.Add(line);

            List<string> output;
            if (queryInfo.queryTextOverride != null)
                output = queryInfo.queryTextOverride(item, terminalItem, defaultDetails);
            else
            {
                var datablock = item.ItemDataBlock;
                output = new()
                {
                    "----------------------------------------------------------------",
                    "CONSUMABLE - " + datablock.terminalItemLongName.ToString()
                };
                if (datablock.ConsumableAmmoMax > 0 && queryInfo.ammoRel)
                    output.Add("CAPACITY: " + (item.pItemData.custom.ammo / datablock.ConsumableAmmoMax).ToString("P0"));
                else
                    output.Add("CAPACITY: " + item.pItemData.custom.ammo.ToString("N0"));
                output.AddRange(defaultDetails);
            }

            foreach (var line in output)
                interpreter.AddOutput(line, spacing: false);
        }

        public static bool HasTerminal(iTerminalItem terminalItem) => _terminalInfo.ContainsKey(terminalItem.Pointer);

        internal static void OnLevelCleanup()
        {
            _terminalInfo.Clear();
        }
    }
}
