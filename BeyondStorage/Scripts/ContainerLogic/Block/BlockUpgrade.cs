using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.ContainerLogic.Block;

public class BlockUpgrade {
    // Used By:
    //      ItemActionRepair.CanRemoveRequiredResource
    //          Block Upgrade - Resources Available Check (called by ItemActionRepair: .ExecuteAction() and .RemoveRequiredResource())
    public static int BlockUpgradeGetItemCount(ItemValue itemValue) {
        // skip if not enabled
        if (!ModConfig.EnableForBlockUpgrade()) return 0;
        var result = ContainerUtils.GetItemCount(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockUpgradeGetItemCount | item {itemValue.ItemClass.GetItemName()}; count {result}");
        return result;
    }

    // Used By:
    //      ItemActionRepair.RemoveRequiredResource
    //          Block Upgrade - Remove items
    public static int BlockUpgradeRemoveRemaining(int currentCount, ItemValue itemValue, int requiredCount) {
        // skip if not enabled
        if (!ModConfig.EnableForBlockUpgrade()) return currentCount;
        // currentCount is previous amount removed by DecItem
        // requiredCount is total required (before last decItem)
        // return early if we already have enough
        if (currentCount == requiredCount) return currentCount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockUpgradeRemoveRemaining | item {itemValue.ItemClass.GetItemName()}; currentCount {currentCount}; requiredCount {requiredCount}");
        var removedFromStorage = ContainerUtils.RemoveRemaining(itemValue, requiredCount - currentCount);
        // add amount removed from storage to previous removed count to update result
        var result = currentCount + removedFromStorage;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"BlockUpgradeRemoveRemaining | item {itemValue.ItemClass.GetItemName()}; removed {removedFromStorage}; new result {result}");
        return result;
    }
}