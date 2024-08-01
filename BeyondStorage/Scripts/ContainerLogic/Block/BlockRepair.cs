using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.ContainerLogic.Block;

public class BlockRepair {
    // Used By:
    //      ItemActionRepair.canRemoveRequiredItem
    //          Block Repair - Resources Available Check
    public static int BlockRepairGetItemCount(ItemValue itemValue) {
        // return early if not enabled for block repair
        if (!ModConfig.EnableForBlockRepair()) return 0;
        var result = ContainerUtils.GetItemCount(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockRepairGetItemCount | item {itemValue.ItemClass.GetItemName()}; result {result}");
        return result;
    }

    // Used By:
    //      ItemActionRepair.removeRequiredItem
    //          Block Repair - remove items on repair
    public static int BlockRepairRemoveRemaining(int currentCount, ItemStack itemStack) {
        // return early if not enabled for block repair
        if (!ModConfig.EnableForBlockRepair()) return currentCount;
        // itemStack.count is total amount needed
        // currentCount is the amount removed previously in last DecItem
        var stillNeed = itemStack.count - currentCount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockRepairRemoveRemaining | itemStack {itemStack.itemValue.ItemClass.GetItemName()}; currentCount {currentCount}; stillNeed {stillNeed} ");
        // Skip if already 0
        if (stillNeed == 0)
            return currentCount;
        // Add amount removed from storage to last amount removed to update result
        var result = currentCount + ContainerUtils.RemoveRemaining(itemStack.itemValue, stillNeed);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockRepairRemoveRemaining | updated Count {result}");
        return result;
    }
}