﻿using BeyondStorage.Scripts.Common;

namespace BeyondStorage.Scripts.Block;

public class BlockRepair {
    // Used By:
    //      ItemActionRepair.canRemoveRequiredItem
    //          Block Repair - Resources Available Check
    public static int BlockRepairGetItemCount(ItemValue itemValue) {
        var result = ContainerUtils.GetItemCount(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockRepairGetItemCount | item {itemValue.ItemClass.GetItemName()}; result {result}");
        return result;
    }

    // Used By:
    //      ItemActionRepair.removeRequiredItem
    //          Block Repair - remove items on repair
    public static int BlockRepairRemoveRemaining(int currentCount, ItemStack itemStack) {
        var stillNeed = itemStack.count - currentCount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockRepairRemoveRemaining | itemStack {itemStack.itemValue.ItemClass.GetItemName()}; currentCount {currentCount}; stillNeed {stillNeed} ");
        // Skip if already 0
        if (stillNeed == 0)
            return currentCount;
        var result = currentCount + ContainerUtils.RemoveRemaining(itemStack.itemValue, stillNeed);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockRepairRemoveRemaining | updated Count {result}");
        return result;
    }
}