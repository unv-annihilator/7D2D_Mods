using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Item;

[HarmonyPatch(typeof(XUiM_PlayerInventory))]
public class XUiMPlayerInventoryCommonPatches {
    //  Adds another check after `num -= this.toolbelt.GetItemCount(_itemStacks[index].itemValue);` in `HasItems`
    //
    // BEFORE:
    //  if (num > 0)
    //    num -= this.toolbelt.GetItemCount(_itemStacks[index].itemValue);
    //  if (num > 0)
    //    return false;
    //
    // AFTER:
    //  if (num > 0)
    //    num -= this.toolbelt.GetItemCount(_itemStacks[index].itemValue);
    //  if (num > 0)
    //    num -= ContainerUtils.GetItemCount(_itemStacks[index].itemValue);
    //  if (num > 0)
    //    return false;
    //

    // Used for:
    //          Item Crafting (has items only, does not handle remove)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.HasItems))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> XUiM_PlayerInventory_HasItems_Patch(
        IEnumerable<CodeInstruction> instructions) {
        LogUtil.Info("Transpiling XUiM_PlayerInventory.HasItems");
        var codes = new List<CodeInstruction>(instructions);
        var set = false;
        for (var i = 0; i < codes.Count; i++) {
            if (i <= 0 || i >= codes.Count - 1 || codes[i].opcode != OpCodes.Ldc_I4_0 ||
                codes[i + 1].opcode != OpCodes.Ret)
                continue;

            if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog("Patching XUiM_PlayerInventory.HasItems");

            List<CodeInstruction> newCode = [
                // num
                new CodeInstruction(OpCodes.Ldloc_1),
                // _itemStacks
                new CodeInstruction(OpCodes.Ldarg_1),
                // index
                new CodeInstruction(OpCodes.Ldloc_0),
                // _itemStacks[index]
                new CodeInstruction(OpCodes.Callvirt, AccessTools.IndexerGetter(typeof(List<ItemStack>), [typeof(int)])),
                // _itemStacks[index].itemValue
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemStack), nameof(ItemStack.itemValue))),
                // ContainerUtils.GetItemCount(_itemStacks[index].itemValue)
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetItemCountForItem))),
                // -=
                new CodeInstruction(OpCodes.Sub),
                // num -= ContainerUtils.GetItemCount(_itemStacks[index].itemValue)
                new CodeInstruction(OpCodes.Stloc_1),
                // ldloc.1      // num
                codes[i - 3].Clone(),
                // ldc.i4.0
                codes[i - 2].Clone(),
                // ble.s        <Label>
                codes[i - 1].Clone()
                // last 3 lines: if (num > 0) -> <Label>
            ];
            codes.InsertRange(i, newCode);
            set = true;
            break;
        }

        if (!set)
            LogUtil.Error("Failed to patch XUiM_PlayerInventory.HasItems");
        else
            LogUtil.Info("Successfully patched XUiM_PlayerInventory.HasItems");

        return codes.AsEnumerable();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.RemoveItems))]
    private static void XUiM_PlayerInventory_RemoveItems_Prefix(IList<ItemStack> _itemStacks, int _multiplier) {
        if (!BeyondStorage.Config.isDebug) return;

        foreach (var itemStack in _itemStacks) {
            var num = itemStack.count * _multiplier;
            LogUtil.DebugLog($"Need {num} {itemStack.itemValue.ItemClass.GetItemName()}");
        }
    }

    // Used For:
    //          Item Crafting (Remove items on craft)
    //          Item Repair (Remove items on repair)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.RemoveItems))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> XUiM_PlayerInventory_RemoveItems_Patch(
        IEnumerable<CodeInstruction> instructions) {
        LogUtil.Info("Transpiling XUiM_PlayerInventory.RemoveItems");
        var codes = new List<CodeInstruction>(instructions);
        var set = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(Inventory), nameof(Inventory.DecItem)))
                continue;

            set = true;
            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Patching XUiM_PlayerInventory.RemoveItems");

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
                // IL_0052: ldc.i4.1
                new CodeInstruction(codes[i - 2].Clone()),
                // IL_0053: ldarg.3      // _removedItems
                new CodeInstruction(codes[i - 1].Clone()),
                // ContainerUtils.RemoveRemainingForCraft(Inventory.DecItem(...), _itemStack[index].itemValue, _count2, _removedItems)
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.RemoveRemainingForCraft)))
            ];
            codes.InsertRange(i + 1, newCode);
            break;
        }

        if (!set)
            LogUtil.Error("Failed to patch XUiM_PlayerInventory.RemoveItems");
        else
            LogUtil.Info("Successfully patched XUiM_PlayerInventory.RemoveItems");

        return codes.AsEnumerable();
    }
}