using LevelGeneration;
using System;
using System.Collections.Generic;
using TerminalConsumables.API;
using TerminalQueryAPI;

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

            return QueryableAPI.ModifyQueryableItem(terminalItem, GetQueryDelegate(queryInfo.item, terminalItem, queryInfo.ammoRel, queryInfo.queryTextOverride));
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

            // For still tracking the iteminlevel when modifying
            _terminalInfo.Add(terminalItem.Pointer, new QueryInfo { item = item, ammoRel = ammoRel, queryTextOverride = queryOverride });

            QueryDelegate del = GetQueryDelegate(item, terminalItem, ammoRel, queryOverride);
            QueryableAPI.RegisterQueryableItem(terminalItem, del);

            return terminalItem;
        }

        public static QueryDelegate GetQueryDelegate(ItemInLevel item, iTerminalItem terminalItem, bool ammoRel = true, QueryTextOverride? queryOverride = null)
        {
            var consumable = item.TryCast<ConsumablePickup_Core>();
            if (queryOverride != null)
                return (List<string> defaultDetails) => queryOverride(item, terminalItem, defaultDetails); // Has override
            else if (consumable != null)
                return (List<string> defaultDetails) => GetConsumableQueryInfo(defaultDetails, consumable, ammoRel); // Is consumable
            else 
                return (List<string> defaultDetails) => defaultDetails; // Default
        }

        public static List<string> GetConsumableQueryInfo(List<string> defaultDetails, ConsumablePickup_Core consumable,  bool ammoRel = true)
        {
            var datablock = consumable.ItemDataBlock;
            List<string> output = new()
            {
                "----------------------------------------------------------------",
                "CONSUMABLE - " + datablock.terminalItemLongName.ToString()
            };

            if (!datablock.GUIShowAmmoInfinite) // for ex LRFs 
            {
                if (datablock.ConsumableAmmoMax > 0 && ammoRel)
                    output.Add("CAPACITY: " + (consumable.pItemData.custom.ammo / datablock.ConsumableAmmoMax).ToString("P0"));
                else
                    output.Add("CAPACITY: " + consumable.pItemData.custom.ammo.ToString("N0"));
            }
            
            output.AddRange(defaultDetails);

            return output;
        }

        internal static void OnLevelCleanup()
        {
            _terminalInfo.Clear();
        }
    }
}
