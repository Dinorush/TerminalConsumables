using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TerminalConsumables
{
    public static class PickupManager
    {
        private static readonly Dictionary<IntPtr, (ConsumablePickup_Core pickup, iTerminalItem terminalItem, string itemKey)> _terminalItems = new();
        private static readonly Dictionary<IntPtr, Func<List<string>, List<string>>> _queryText = new();

        public static void RegisterTerminalInfo(ConsumablePickup_Core pickup, LG_GenericTerminalItem terminalItem, string itemKey, Func<List<string>, List<string>> queryText)
        {
            _terminalItems.Add(pickup.Pointer, (pickup, terminalItem.Cast<iTerminalItem>(), itemKey));
            _queryText.Add(terminalItem.Pointer, queryText);
        }
        public static bool TryGetTerminalInfo(ConsumablePickup_Core pickup, [MaybeNullWhen(false)] out (ConsumablePickup_Core pickup, iTerminalItem terminalItem, string itemKey) info) => _terminalItems.TryGetValue(pickup.Pointer, out info);

        public static bool TryGetQueryText(iTerminalItem terminalItem, List<string> defaultDetails, [MaybeNullWhen(false)] out List<string> list)
        {
            if (!_queryText.TryGetValue(terminalItem.Pointer, out var func))
            {
                list = null;
                return false;
            }

            list = func(defaultDetails);
            return true;
        }

        public static bool HasTerminal(iTerminalItem terminalItem) => _queryText.ContainsKey(terminalItem.Pointer);

        internal static void OnLevelCleanup()
        {
            _terminalItems.Clear();
            _queryText.Clear();
        }
    }
}
