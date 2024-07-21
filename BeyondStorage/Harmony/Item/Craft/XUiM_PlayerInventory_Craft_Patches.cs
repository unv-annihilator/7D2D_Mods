using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.Item;
using HarmonyLib;

namespace BeyondStorage.Item.Craft;

[HarmonyPatch(typeof(XUiM_PlayerInventory))]
public class XUiMPlayerInventoryCraftPatches {
    // Used for:
    //          Item Crafting (has items only, does not handle remove)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.HasItems))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> XUiM_PlayerInventory_HasItems_Patch(IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(XUiM_PlayerInventory)}.{nameof(XUiM_PlayerInventory.HasItems)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var set = false;
        for (var i = 0; i < codes.Count; i++) {
            if (i <= 0 || i >= codes.Count - 1 || codes[i].opcode != OpCodes.Ldc_I4_0 || codes[i + 1].opcode != OpCodes.Ret)
                continue;

            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Patching {targetMethodString}");

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
                    AccessTools.Method(typeof(ItemCraft), nameof(ItemCraft.HasItemGetItemCount))),
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
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}