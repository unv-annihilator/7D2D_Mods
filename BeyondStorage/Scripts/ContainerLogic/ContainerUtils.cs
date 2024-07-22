using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeyondStorage.Scripts.Common;
using Platform;
using UnityEngine;

namespace BeyondStorage.Scripts.ContainerLogic;

public static class ContainerUtils {
    private static ConcurrentDictionary<Vector3i, int> _lockedTileEntities;
    public static int LastLockedCount { get; set; }

    public static void Init() {
        _lockedTileEntities = new ConcurrentDictionary<Vector3i, int>();
    }

    public static void Cleanup() {
        _lockedTileEntities.Clear();
    }

    public static IEnumerable<ItemStack> GetItemStacks() {
        return GetAvailableStorages().SelectMany(lootable => lootable.items);
    }

    public static bool HasItem(ItemValue itemValue) {
        return GetAvailableStorages().Select(lootable => lootable.items).Any(itemStacks => itemStacks.Any(stack => stack.itemValue.type == itemValue.type));
    }

    public static int GetItemCount(ItemValue itemValue) {
        return (from tileEntityLootable in GetAvailableStorages()
            from itemStack in tileEntityLootable.items
            where itemStack.itemValue.type == itemValue.type
            select itemStack.count).Sum();
    }

    private static IEnumerable<ITileEntityLootable> GetAvailableStorages() {
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        var playerPos = player.position;
        var configRange = BeyondStorage.Config.range;
        var configOnlyCrates = BeyondStorage.Config.onlyStorageCrates;
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
            LogUtil.DebugLog($"TEL: {tileEntityLootable}; Locked Count: {_lockedTileEntities.Count}; {tileEntity.IsUserAccessing()}");
#endif
            // If we have locked entities
            if (_lockedTileEntities.Count > 0)
                // Skip if player already accessing the storage
                if (_lockedTileEntities.ContainsKey(tileEntityLootable.ToWorldPos()) && _lockedTileEntities[tileEntityLootable.ToWorldPos()] != player.entityId)
                    continue;

            // Skip if the container can be locked
            if (tileEntity.TryGetSelfOrFeature(out ILockable tileLockable))
                // And is locked, and the player doesn't have access
                if (tileLockable.IsLocked() && !tileLockable.IsUserAllowed(internalLocalUserIdentifier))
                    continue;
            // If entity is in range (or range is set infinite)
            if (configRange <= 0 || Vector3.Distance(playerPos, tileEntity.ToWorldPos()) < configRange)
                yield return tileEntityLootable;
        }
    }

    public static int RemoveRemaining(ItemValue itemValue, int requiredAmount, bool ignoreModdedItems = false, IList<ItemStack> removedItems = null) {
        var num = requiredAmount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"RemoveRemaining | Trying to remove {requiredAmount} {itemValue.ItemClass.GetItemName()}");

        if (requiredAmount <= 0) return requiredAmount;

        foreach (var tileEntityLootable in GetAvailableStorages()) {
            var items = tileEntityLootable.items;
            for (var index = 0; requiredAmount > 0 && index < items.Length; ++index) {
                if (items[index].itemValue.type != itemValue.type || ignoreModdedItems &&
                    items[index].itemValue.HasModSlots &&
                    items[index].itemValue.HasMods()) {
                    continue;
                }

                if (ItemClass.GetForId(items[index].itemValue.type).CanStack()) {
                    var itemCount = items[index].count;
                    var countToRemove = itemCount >= requiredAmount ? requiredAmount : itemCount;
                    if (BeyondStorage.Config.isDebug) {
                        LogUtil.DebugLog($"Loc: {tileEntityLootable.ToWorldPos()}, Value: {tileEntityLootable}");
                        LogUtil.DebugLog($"Item Count: {itemCount} Count To Remove: {countToRemove}");
                        LogUtil.DebugLog($"Item Count Before: {items[index].count}");
                    }

                    removedItems?.Add(new ItemStack(items[index].itemValue.Clone(), countToRemove));
                    items[index].count -= countToRemove;
                    requiredAmount -= countToRemove;
                    if (BeyondStorage.Config.isDebug) {
                        LogUtil.DebugLog($"Item Count After: {items[index].count}");
                        LogUtil.DebugLog($"Required After: {requiredAmount}");
                    }

                    if (items[index].count <= 0) items[index].Clear();

                    tileEntityLootable.SetModified();
                } else {
                    removedItems?.Add(items[index].Clone());
                    items[index].Clear();
                    --requiredAmount;
                    tileEntityLootable.SetModified();
                }
            }
        }

        var result = num - requiredAmount;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"RemoveRemaining | Removed {result} {itemValue.ItemClass.GetItemName()}");

        return result;
    }

    public static void UpdateLockedTEs(Dictionary<Vector3i, int> lockedTileEntities) {
        _lockedTileEntities.Clear();
        lockedTileEntities.CopyTo(_lockedTileEntities);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"UpdateLockedTEs: newCount {lockedTileEntities.Count}");
    }
}