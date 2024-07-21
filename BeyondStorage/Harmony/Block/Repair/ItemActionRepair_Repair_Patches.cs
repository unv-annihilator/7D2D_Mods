using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.ContainerLogic.Block;
using HarmonyLib;

namespace BeyondStorage.Block.Repair;

[HarmonyPatch(typeof(ItemActionRepair))]
public class ItemActionRepairPatches {
    // Used For:
    //          Block Repair (resources available check)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionRepair.canRemoveRequiredItem))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> ItemActionRepair_canRemoveRequiredItem_Patch(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        if (!BeyondStorage.Config.enableForBlockRepair) return instructions;
        var targetMethodString = $"{typeof(ItemActionRepair)}.{nameof(ItemActionRepair.canRemoveRequiredItem)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(Bag), nameof(Bag.GetItemCount)))
                continue;

            found = true;
            if (LogUtil.IsDebug()) LogUtil.DebugLog("Adding method to count items from all storages");

            List<CodeInstruction> newCode = [];
            // == New ==
            var newLabel = generator.DefineLabel();
            // New jump to our new section of code if the previous check failed
            newCode.Add(new CodeInstruction(OpCodes.Blt_S, newLabel));
            // else
            newCode.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            newCode.Add(new CodeInstruction(OpCodes.Ret));
            // Create our first bit of new code
            // _itemStack
            var ci = new CodeInstruction(OpCodes.Ldarg_2);
            // Apply our label to this CI
            ci.labels.Add(newLabel);
            newCode.Add(ci);
            // Get itemValue
            newCode.Add(new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(ItemStack), nameof(ItemStack.itemValue))));
            // GetItemCount(itemValue)
            newCode.Add(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BlockRepair), nameof(BlockRepair.BlockRepairGetItemCount))));
            // _itemStack
            newCode.Add(new CodeInstruction(OpCodes.Ldarg_2));
            // _itemStack.count
            newCode.Add(new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(ItemStack), nameof(ItemStack.count))));
            codes.InsertRange(i + 3, newCode);
            // == End New ==

            // == Old - Called even if not needed (enough items in inventory) ==
            // newCode.Add(new CodeInstruction(OpCodes.Ldarg_2));
            // newCode.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.AddAllStoragesCountItemStack))));
            // codes.InsertRange(i + 1, newCode);
            // == End Old ==
            break;
        }

        if (!found)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }

    // Used For:
    //          Block Repair (remove items on repair)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionRepair.removeRequiredItem))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> ItemActionRepair_removeRequiredItem_Patch(
        IEnumerable<CodeInstruction> instructions) {
        if (!BeyondStorage.Config.enableForBlockRepair) return instructions;
        var targetMethodString = $"{typeof(ItemActionRepair)}.{nameof(ItemActionRepair.removeRequiredItem)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt ||
                (MethodInfo)codes[i].operand != AccessTools.Method(typeof(Bag), nameof(Bag.DecItem)))
                continue;

            found = true;
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Patching {targetMethodString}");

            List<CodeInstruction> newCode = [
                // _itemStack
                new CodeInstruction(OpCodes.Ldarg_2),
                // BlockRepair.BlockRepairRemoveRemaining(Bag::DecItem(), _itemStack)
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(BlockRepair), nameof(BlockRepair.BlockRepairRemoveRemaining)))
            ];
            codes.InsertRange(i + 1, newCode);
            break;
        }

        if (!found)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}