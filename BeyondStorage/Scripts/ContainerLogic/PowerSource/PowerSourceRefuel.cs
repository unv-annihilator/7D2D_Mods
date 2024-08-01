using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.ContainerLogic.PowerSource;

public static class PowerSourceRefuel {
    public static int RefuelRemoveRemaining(ItemValue itemValue, int lastRemoved, int totalNeeded) {
        if (!ModConfig.EnableForGeneratorRefuel()) return lastRemoved;
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog("RefuelRemoveRemaining");
#endif
        // return early if we already have enough
        if (lastRemoved == totalNeeded) return lastRemoved;
        // update new required amount count removing last removed from total needed
        var newReqCount = totalNeeded - lastRemoved;
        // attempt to remove items from storage
        var removedFromStorage = ContainerUtils.RemoveRemaining(itemValue, newReqCount);
        if (LogUtil.IsDebug())
            LogUtil.DebugLog(
                $"RefuelRemoveRemaining - item {itemValue.ItemClass.GetItemName()}; lastRemoved {lastRemoved}; totalNeeded {totalNeeded}; newReqCount {newReqCount}; removedFromStorage {removedFromStorage}; updated result {lastRemoved + removedFromStorage}");
        // add what removed from storage to last removed count
        return lastRemoved + removedFromStorage;
    }
}