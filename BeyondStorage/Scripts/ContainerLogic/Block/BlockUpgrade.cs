using BeyondStorage.Scripts.Common;

namespace BeyondStorage.Scripts.ContainerLogic.Block;

public class BlockUpgrade {
    // Used By:
    //      ItemActionRepair.CanRemoveRequiredResource
    //          Block Upgrade - Resources Available Check (called by ItemActionRepair: .ExecuteAction() and .RemoveRequiredResource())
    public static int BlockUpgradeGetItemCount(ItemValue itemValue) {
        var result = ContainerUtils.GetItemCount(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockUpgradeGetItemCount | item {itemValue.ItemClass.GetItemName()}; count {result}");
        return result;
    }

    // Used By:
    //      ItemActionRepair.RemoveRequiredResource
    //          Block Upgrade - Remove items
    public static int BlockUpgradeRemoveRemaining(int currentCount, ItemValue itemValue, int requiredCount) {
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockUpgradeRemoveRemaining | item {itemValue.ItemClass.GetItemName()}; currentCount {currentCount}; requiredCount {requiredCount}");
        if (currentCount == requiredCount) return currentCount;
        var removedFromStorage = ContainerUtils.RemoveRemaining(itemValue, requiredCount - currentCount);
        var result = currentCount + removedFromStorage;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockUpgradeRemoveRemaining | item {itemValue.ItemClass.GetItemName()}; removed {removedFromStorage}; new result {result}");
        return result;
    }
}