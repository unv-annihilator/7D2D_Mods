using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Server;
using BeyondStorage.Scripts.Utils;
using Platform;
using UnityEngine;

namespace BeyondStorage.Scripts.ContainerLogic;

public static class ContainerUtils {
    public static ConcurrentDictionary<Vector3i, int> LockedTileEntities { get; private set; }

    public static void Init() {
        ServerUtils.HasServerConfig = false;
        LockedTileEntities = new ConcurrentDictionary<Vector3i, int>();
    }

    public static void Cleanup() {
        ServerUtils.HasServerConfig = false;
        LockedTileEntities?.Clear();
    }

    // Client Update
    public static void UpdateLockedTEs(Dictionary<Vector3i, int> lockedTileEntities) {
        LockedTileEntities = new ConcurrentDictionary<Vector3i, int>(lockedTileEntities);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"UpdateLockedTEs: newCount {lockedTileEntities.Count}");
    }

    public static IEnumerable<ItemStack> GetItemStacks() {
        // get container storage
        var containerStorage = GetAvailableStorages();
        // get results if storage was not null; otherwise return empty list
        var containerResults = containerStorage == null ? new List<ItemStack>().AsEnumerable() : containerStorage.SelectMany(lootable => lootable.items);
        // return container results if we're not pulling from vehicles
        if (!ModConfig.PullFromVehicleStorage()) return containerResults;
        // == start vehicle code ==
        // init vehicleResults
        var vehicleResults = new List<ItemStack>().AsEnumerable();
        // get available vehicle storage
        var vehicleStorage = VehicleUtils.GetAvailableVehicleStorages();
        // check if we got any vehicle storage results
        if (vehicleStorage != null) // set results if we found any storage to pull from
            vehicleResults = vehicleStorage.SelectMany(vehicle => vehicle.bag?.GetSlots());
        // return a concat of both results
        return containerResults.Concat(vehicleResults);
    }

    public static bool HasItem(ItemValue itemValue) {
        // capture container storage
        var containerStorage = GetAvailableStorages();
        // return false if no storage, otherwise return if any is found
        var containerHas = containerStorage != null && containerStorage.Select(lootable => lootable.items).Any(itemStacks => itemStacks.Any(stack => stack.itemValue.type == itemValue.type));
        // return early if container has
        if (containerHas)
            return true;
        // return false if container didn't have and not set to pull from vehicles
        if (!ModConfig.PullFromVehicleStorage())
            return false;
        // == start vehicle code ==
        // get vehicle storage
        var vehicleStorage = VehicleUtils.GetAvailableVehicleStorages();
        // check vehicles storage if it exists, otherwise return false
        return vehicleStorage != null && vehicleStorage.Select(vehicle => vehicle.bag.items).Any(itemStacks => itemStacks.Any(stack => stack.itemValue.type == itemValue.type));
    }

    public static int GetItemCount(ItemValue itemValue) {
        // capture container storages
        var containerStorage = GetAvailableStorages();
        // 0 if no storage, otherwise count the number of items found
        var containerCount = containerStorage == null
            ? 0
            : (from tileEntityLootable in containerStorage from itemStack in tileEntityLootable.items where itemStack.itemValue.type == itemValue.type select itemStack.count).Sum();
        // return early if we're not pulling from vehicles
        if (!ModConfig.PullFromVehicleStorage())
            return containerCount;
        // == start vehicle code ==
        // get vehicle storages
        var vehicleStorage = VehicleUtils.GetAvailableVehicleStorages();
        // count from vehicle storage
        var vehicleCount = vehicleStorage == null
            ? 0
            : vehicleStorage.Select(vehicle => vehicle.bag.items).SelectMany(stacks => stacks).Where(itemStack => itemStack.itemValue.type == itemValue.type).Select(itemStack => itemStack.count)
                .Sum();
        // return combo result
        return containerCount + vehicleCount;
    }

    private static IEnumerable<ITileEntityLootable> GetAvailableStorages() {
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        var playerPos = player.position;
        var configRange = ModConfig.Range();
        var configOnlyCrates = ModConfig.OnlyStorageCrates();
        var internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        // Get a copy of the current chunk cache
        var chunkCacheCopy = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
        // Get a list of tile entities from chunk if not null
        foreach (var tileEntity in chunkCacheCopy.Where(chunk => chunk != null).SelectMany(chunk => chunk.GetTileEntities().list)) {
            // Skip if not a lootable
            if (!tileEntity.TryGetSelfOrFeature(out ITileEntityLootable tileEntityLootable)) continue;
            // Skip if not a storage crate AND config set to only use Storage crates
            if (configOnlyCrates && !tileEntity.TryGetSelfOrFeature(out TEFeatureStorage _)) continue;
            // Skip if not player storage
            if (!tileEntityLootable.bPlayerStorage) continue;
#if DEBUG
            LogUtil.DebugLog($"TEL: {tileEntityLootable}; Locked Count: {LockedTileEntities.Count}; {tileEntity.IsUserAccessing()}");
#endif
            // If we have locked entities
            if (LockedTileEntities.Count > 0)
                // Skip if player already accessing the storage
                if (LockedTileEntities.ContainsKey(tileEntityLootable.ToWorldPos()) && LockedTileEntities[tileEntityLootable.ToWorldPos()] != player.entityId)
                    continue;

            // check if storage is lockable
            if (tileEntity.TryGetSelfOrFeature(out ILockable tileLockable))
                // If storage can be locked, is locked, and the player doesn't have access
                if (tileLockable.IsLocked() && !tileLockable.IsUserAllowed(internalLocalUserIdentifier))
                    continue;

            // Skip empty TEL
            if (tileEntityLootable.IsEmpty()) continue;
            // If entity is in range (or range is set infinite)
            if (configRange <= 0 || Vector3.Distance(playerPos, tileEntity.ToWorldPos()) < configRange)
                yield return tileEntityLootable;
        }
    }

    private static int RemoveItems(ItemStack[] items, ItemValue desiredItem, int requiredAmount, bool ignoreModdedItems = false, IList<ItemStack> removedItems = null) {
        for (var index = 0; requiredAmount > 0 && index < items.Length; ++index) {
            if (items[index].itemValue.type != desiredItem.type || ignoreModdedItems &&
                items[index].itemValue.HasModSlots &&
                items[index].itemValue.HasMods()) {
                continue;
            }

            if (ItemClass.GetForId(items[index].itemValue.type).CanStack()) {
                var itemCount = items[index].count;
                var countToRemove = itemCount >= requiredAmount ? requiredAmount : itemCount;
#if DEBUG
                if (LogUtil.IsDebug()) {
                    // LogUtil.DebugLog($"Loc: {tileEntityLootable.ToWorldPos()}, Value: {tileEntityLootable}");
                    LogUtil.DebugLog($"Item Count: {itemCount} Count To Remove: {countToRemove}");
                    LogUtil.DebugLog($"Item Count Before: {items[index].count}");
                }
#endif
                removedItems?.Add(new ItemStack(items[index].itemValue.Clone(), countToRemove));
                items[index].count -= countToRemove;
                requiredAmount -= countToRemove;
#if DEBUG
                if (LogUtil.IsDebug()) {
                    LogUtil.DebugLog($"Item Count After: {items[index].count}");
                    LogUtil.DebugLog($"Required After: {requiredAmount}");
                }
#endif
                if (items[index].count <= 0) items[index].Clear();
            } else {
                removedItems?.Add(items[index].Clone());
                items[index].Clear();
                --requiredAmount;
            }
        }

        return requiredAmount;
    }

    public static int RemoveRemaining(ItemValue itemValue, int requiredAmount, bool ignoreModdedItems = false, IList<ItemStack> removedItems = null) {
        // return early if we already have enough items removed
        if (requiredAmount <= 0) return requiredAmount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"RemoveRemaining | Trying to remove {requiredAmount} {itemValue.ItemClass.GetItemName()}");
        var originalAmountNeeded = requiredAmount;
        // capture container storage
        var containerStorage = GetAvailableStorages();
        // update storage if it's null to empty list
        containerStorage ??= new List<ITileEntityLootable>().AsEnumerable();
        foreach (var tileEntityLootable in containerStorage) {
            // Remove items from TEL
            var newRequiredAmount = RemoveItems(tileEntityLootable.items, itemValue, requiredAmount, ignoreModdedItems, removedItems);
            // check if we took items from the TEL, if so set modified
            if (requiredAmount != newRequiredAmount) tileEntityLootable.SetModified();
            // update required amount
            requiredAmount = newRequiredAmount;
            // continue if we still need more
            if (requiredAmount > 0) continue;
            // otherwise return early
            break;
        }

        var difference = originalAmountNeeded - requiredAmount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"RemoveRemaining | Containers | Removed {difference} {itemValue.ItemClass.GetItemName()}");
        // if we already have enough return
        if (requiredAmount <= 0) return originalAmountNeeded - requiredAmount;
        // return if we're not checking vehicle storages
        if (!ModConfig.PullFromVehicleStorage()) return originalAmountNeeded - requiredAmount;
        // == start vehicle code ==
        // get current vehicle storages
        var vehicleStorages = VehicleUtils.GetAvailableVehicleStorages();
        // update to new list if null
        vehicleStorages ??= new List<EntityVehicle>().AsEnumerable();
        // check vehicle storages
        foreach (var vehicle in vehicleStorages) {
            // try and remove items from vehicle storage
            var newRequiredAmount = RemoveItems(vehicle.bag.items, itemValue, requiredAmount, ignoreModdedItems, removedItems);
            // if we took something, set the vehicle storage as modified
            if (newRequiredAmount != requiredAmount) vehicle.SetBagModified();
            // update required amount
            requiredAmount = newRequiredAmount;
            // continue if we still need more
            if (requiredAmount > 0) continue;
            // otherwise return early
            break;
        }

        // check difference
        difference = originalAmountNeeded - requiredAmount - difference;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"RemoveRemaining | Vehicles | Removed {difference} {itemValue.ItemClass.GetItemName()}");
        // return originalCountNeeded - newRequiredAmount
        return originalAmountNeeded - requiredAmount;
    }
}