using LevelGeneration;
using System.Collections.Generic;
using TerminalConsumables.Managers;

namespace TerminalConsumables.API
{
    /// <summary>
    /// Returns the lines of text to print for a query command. Each element represents one line.
    /// </summary>
    /// <param name="item">The item being queried.</param>
    /// <param name="terminalItem">The terminal item component of the item being queried.</param>
    /// <param name="defaultDetails">The default details provided by the interpreter, in the format
    /// <code>
    /// ----------------------------------------------------------------
    /// ID: terminalItem.TerminalItemKey
    /// ITEM STATUS: terminalItem.FloorItemStatus
    /// LOCATION: terminalItem.FloorItemLocation
    /// PING STATUS: ...</code>
    /// </param>
    /// <returns></returns>
    public delegate List<string> QueryTextOverride(Item item, iTerminalItem terminalItem, List<string> defaultDetails);

    public static class TerminalAPI
    {
        /// <summary>
        /// Registers the passed item to the terminal system. Queries will print as if the item were a consumable. If the item already has a terminal item component, it will register it as a tracked item with the given settings.
        /// </summary>
        /// <param name="item">The item to register in the terminal system.</param>
        /// <param name="ammoRel">Shows capacity as a percentage when true or the raw value when false.</param>
        /// <param name="queryOverride">Overrides query text with the returned list if not null.</param>
        /// <returns>
        /// A struct containing the newly created iTerminalItem and its item key string.
        /// </returns>
        public static iTerminalItem RegisterTerminalItem(Item item,  bool ammoRel = true, QueryTextOverride? queryOverride = null) => TerminalItemManager.AddTerminalItem(item, ammoRel, queryOverride);

        /// <summary>
        /// Modifies query-related options for the terminal item. If the item is not tracked, it is registered as a tracked item.
        /// </summary>
        /// <param name="terminalItem">The terminal item to modify queries on.</param>
        /// <param name="ammoRel">Shows capacity as a percentage when true or the raw value when false.</param>
        /// <param name="queryOverride">Overrides query text with the returned list if not null.</param>
        /// <returns>
        /// A struct containing the newly created iTerminalItem and its item key string.
        /// </returns>
        public static bool ModifyTerminalItem(iTerminalItem? terminalItem, bool ammoRel = true, QueryTextOverride? queryOverride = null) => TerminalItemManager.ModifyTerminalItem(terminalItem, ammoRel, queryOverride);

        /// <summary>
        /// Modifies query-related options for the terminal item found on the passed item. If the item is not tracked, it is registered as a tracked item.
        /// </summary>
        /// <param name="item">The item to obtain a terminal item from and modify queries on.</param>
        /// <param name="ammoRel">Shows capacity as a percentage when true or the raw value when false.</param>
        /// <param name="queryOverride">Overrides query text with the returned list if not null.</param>
        /// <returns>
        /// A struct containing the newly created iTerminalItem and its item key string.
        /// </returns>
        public static bool ModifyTerminalItem(Item item, bool ammoRel = true, QueryTextOverride? queryOverride = null) => ModifyTerminalItem(item.GetComponent<iTerminalItem>(), ammoRel, queryOverride);
    }
}
