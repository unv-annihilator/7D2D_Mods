using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.Item;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace BeyondStorage.Item;

[HarmonyPatch(typeof(XUiM_PlayerInventory))]
public class XUiMPlayerInventoryCommonPatches {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.RemoveItems))]
    private static void XUiM_PlayerInventory_RemoveItems_Prefix(IList<ItemStack> _itemStacks, int _multiplier) {
        if (!LogUtil.IsDebug()) return;
        var targetMethodStr = $"{typeof(XUiM_PlayerInventory)}.{nameof(XUiM_PlayerInventory.RemoveItems)} PREFIX";

        foreach (var itemStack in _itemStacks) {
            var num = itemStack.count * _multiplier;
            LogUtil.DebugLog($"{targetMethodStr} | Need {num} {itemStack.itemValue.ItemClass.GetItemName()}");
        }
    }

    // Used For:
    //          Item Crafting (Remove items on craft)
    //          Item Repair (Remove items on repair)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.RemoveItems))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> XUiM_PlayerInventory_RemoveItems_Patch(
        IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(XUiM_PlayerInventory)}.{nameof(XUiM_PlayerInventory.RemoveItems)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var set = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(Inventory), nameof(Inventory.DecItem)))
                continue;

            set = true;
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Patching {targetMethodString}");

            List<CodeInstruction> newCode = [
                // ldarg.1      // _itemStacks
                new CodeInstruction(codes[i - 7].Clone()),
                // ldloc.0      // index
                new CodeInstruction(codes[i - 6].Clone()),
                // callvirt     instance !0/*class ItemStack*/ class [mscorlib]System.Collections.Generic.IList`1<class ItemStack>::get_Item(int32)
                new CodeInstruction(codes[i - 5].Clone()),
                // ldfld        class ItemValue ItemStack::itemValue
                new CodeInstruction(codes[i - 4].Clone()),
                // IL_0051: ldloc.1      // _count2
                new CodeInstruction(codes[i - 3].Clone()),
                // IL_0052: ldc.i4.1    true (ignoreModded)
                new CodeInstruction(codes[i - 2].Clone()),
                // IL_0053: ldarg.3      // _removedItems
                new CodeInstruction(codes[i - 1].Clone()),
                // ItemCommon.ItemRemoveRemaining(Inventory.DecItem(...), _itemStack[index].itemValue, _count2, true, _removedItems)
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ItemCommon), nameof(ItemCommon.ItemRemoveRemaining)))
            ];
            codes.InsertRange(i + 1, newCode);
            break;
        }

        if (!set)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}