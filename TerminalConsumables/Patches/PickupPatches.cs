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

        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.Query))]
        [HarmonyPrefix]
        static bool Prefix(LG_ComputerTerminalCommandInterpreter __instance, string param1)
        {
            if (!LG_LevelInteractionManager.TryGetTerminalInterface(param1.ToUpper(), __instance.m_terminal.SpawnNode.m_dimension.DimensionIndex, out var target)) return true;

            if (!TerminalItemManager.HasTerminal(target)) return true;

            __instance.AddOutput(TerminalLineType.SpinningWaitDone, "Querying " + param1.ToUpper(), 3f);
            __instance.AddOutputEmptyLine();
            TerminalItemManager.DoQueryOutput(target, __instance);
            __instance.AddOutputEmptyLine();
            return false;
        }
    }
}
