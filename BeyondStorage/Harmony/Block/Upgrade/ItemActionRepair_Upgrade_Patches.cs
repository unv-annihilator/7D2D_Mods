using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Block.Upgrade;

[HarmonyPatch(typeof(ItemActionRepair))]
public class ItemActionRepairUpgradePatches {
    // Used For:
    //          Block Upgrade (Check for enough items)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionRepair.CanRemoveRequiredResource))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> ItemActionRepair_CanRemoveRequiredResource_Patch(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        if (!BeyondStorage.Config.enableForBlockUpgrade) return instructions;
        var targetMethodString = $"{typeof(ItemActionRepair)}.{nameof(ItemActionRepair.CanRemoveRequiredResource)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(Bag), nameof(Bag.GetItemCount)))
                continue;

            found = true;
            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Adding method to count items from all storages");

            List<CodeInstruction> newCode = new();
            // == 'Proper' Code / Only Called When Needed ==
            var newLabel = generator.DefineLabel();
            // blt.s    [newLabel]
            newCode.Add(new CodeInstruction(OpCodes.Blt_S, newLabel));
            // ldc.i4.1
            newCode.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            // ret
            newCode.Add(new CodeInstruction(OpCodes.Ret));
            // ldloc.s  _itemValue [newLabel]
            var ci = new CodeInstruction(codes[i - 4].opcode, codes[i - 4].operand);
            ci.labels.Add(newLabel);
            newCode.Add(ci);
            // ContainerUtils.GetItemCount(_itemValue)
            newCode.Add(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetItemCount))));
            // Moves result onto stack
            newCode.Add(new CodeInstruction(OpCodes.Ldloc_3));
            codes.InsertRange(i + 2, newCode);
            // == END 'Proper' Code ==

            // == Quick Hook / Called even if inventory has enough already ==
            // newCode.Add(new CodeInstruction(codes[i - 4].opcode, codes[i - 4].operand));
            // newCode.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetItemCount2))));
            // codes.InsertRange(i + 1, newCode);
            // == END Quick Hook ==
            break;
        }

        if (!found)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }

    // Used For:
    //          Block Upgrade (Remove items)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionRepair.RemoveRequiredResource))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> ItemActionRepair_RemoveRequiredResource_Patch(
        IEnumerable<CodeInstruction> instructions) {
        if (!BeyondStorage.Config.enableForBlockUpgrade) return instructions;
        var targetMethodString = $"{typeof(ItemActionRepair)}.{nameof(ItemActionRepair.RemoveRequiredResource)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            // if (data.holdingEntity.bag.DecItem(_itemValue, result) != result)
            if (codes[i].opcode != OpCodes.Callvirt ||
                (MethodInfo)codes[i].operand != AccessTools.Method(typeof(Bag), nameof(Bag.DecItem)))
                continue;

            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Adding method to remove items from all storages");

            found = true;
            List<CodeInstruction> newCode = [
                // _itemValue
                new CodeInstruction(OpCodes.Ldloc_1),
                // _result
                new CodeInstruction(OpCodes.Ldloc_2),
                // ContainerUtils.RemoveRemainingForUpgrade(bag.DecItem(...), _itemValue, _result)
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.RemoveRemainingForUpgrade)))
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