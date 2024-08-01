using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.ContainerLogic.Vehicle;

public static class VehicleRefuel {
    public static int VehicleRefuelRemoveRemaining(ItemValue itemValue, int lastRemovedCount, int totalRequired) {
        // skip if not enabled
        if (!ModConfig.EnableForVehicleRefuel()) return lastRemovedCount;
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog("VehicleRefuelRemoveRemaining");
#endif
        // skip if already at required amount
        if (lastRemovedCount == totalRequired) return lastRemovedCount;
        var newRequiredCount = totalRequired - lastRemovedCount;
        var removedFromStorage = ContainerUtils.RemoveRemaining(itemValue, newRequiredCount);
        if (LogUtil.IsDebug())
            LogUtil.DebugLog(
                $"VehicleRefuelRemoveRemaining - item {itemValue.ItemClass.GetItemName()}; lastRemoved {lastRemovedCount}; totalRequired {totalRequired}; newReqAmt {newRequiredCount}; removedFromStorage {removedFromStorage}; newResult {lastRemovedCount + removedFromStorage}");
        // return new refueled count
        return lastRemovedCount + removedFromStorage;
    }

    public static bool CanRefuel(EntityVehicle vehicle, bool originalResult) {
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog("VehicleRefuel.CanRefuel");
#endif
        // return early if already able to refuel from inventory
        if (originalResult) return true;
        // attempt to get fuelItem, return false if unable to find
        var fuelItem = vehicle.GetVehicle().GetFuelItem();
        if (fuelItem == "") return false;
        var fuelItemValue = ItemClass.GetItem(fuelItem);
        var storageHas = ContainerUtils.HasItem(fuelItemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"VehicleRefuel.CanRefuel - fuelItem {fuelItem}; storageHas {storageHas}");
        return storageHas;
    }
}