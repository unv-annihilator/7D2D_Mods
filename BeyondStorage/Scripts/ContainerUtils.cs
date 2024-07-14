using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeyondStorage.Scripts.Common;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace BeyondStorage.Scripts;

public static class ContainerUtils {
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static Dictionary<Vector3i, ITileEntityLootable> _currentStorageDict = new();

    public static void Init() {
        _currentStorageDict.Clear();
    }

    public static void Cleanup() {
        _currentStorageDict.Clear();
    }

    public static int AddAllStoragesCountEntry(int count, XUiC_IngredientEntry entry) {
        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog(
                $"AddAllStoragesCountEntry | count {count} |  entry {entry.Ingredient.itemValue.ItemClass.GetItemName()}");

        return GetItemCount2(count, entry.Ingredient.itemValue);
    }

    private static int GetItemCount2(int count, ItemValue item) {
        ReloadStorages();
        if (_currentStorageDict.Count == 0) return count;

        count += _currentStorageDict.Select(kvp => kvp.Value.items)
            .Sum(items => items.Where(t => t.itemValue.type == item.type).Sum(t => t.count));
        return count;
    }

    public static ItemStack[] GetAllStorageStacksArrays(ItemStack[] items) {
        // TODO: Frequently called
        // if (BeyondStorage.Config.isDebug) {
        //     LogUtil.DebugLog($"GetAllStorageStacksArrays");
        // }
        return GetAllStorageStacks(items.ToList()).ToArray();
    }

    private static List<ItemStack> GetAllStorageStacks(List<ItemStack> items) {
        // TODO: Frequently called
        // if (BeyondStorage.Config.isDebug) {
        //     LogUtil.DebugLog($"GetAllStorageStacks");
        // }
        AddAllStorageStacks(items);
        return items;
    }

    public static void AddAllStorageStacks(List<ItemStack> items) {
        // TODO: Frequently called
        // if (BeyondStorage.Config.isDebug) {
        //     LogUtil.DebugLog($"AddAllStorageStacks");
        // }

        ReloadStorages();
        if (_currentStorageDict.Count == 0) return;

        foreach (var kvp in _currentStorageDict) {
            items.AddRange(kvp.Value.items);
        }
    }

    public static int GetItemCount(ItemValue itemValue) {
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"GetItemCount | item {itemValue.ItemClass.GetItemName()}");

        ReloadStorages();
        return _currentStorageDict.Count == 0
            ? 0
            : _currentStorageDict.Select(kvp => kvp.Value.items).Sum(items => GetItemCountFromSlots(items, itemValue));
    }

    private static int GetItemCountFromSlots(ItemStack[] slots, ItemValue itemValue) {
        return slots.Where(t => t.itemValue.type == itemValue.type).Sum(t => t.count);
    }

    public static int GetTrueItemRepairCount(ItemValue itemValue, int currentCount) {
        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog(
                $"GetTrueItemRepairCount | item {itemValue.ItemClass.GetItemName()} | currentCount {currentCount}");

        // Skip if we already have enough in inventory
        if (currentCount * itemValue.ItemClass.RepairAmount.Value <= 0) {
            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("GetTrueItemRepairCount | adding nearby inventory");

            return currentCount + GetItemCount(itemValue);
        }

        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("GetTrueItemRepairCount | returning current count");

        return currentCount;
    }

    public static int RemoveRemainingForUpgrade(int currentCount, ItemValue itemValue, int requiredCount) {
        if (currentCount == requiredCount) {
            if (BeyondStorage.Config.isDebug)
                LogUtil.DebugLog(
                    $"RemoveRemainingForUpgrade | Skipping {itemValue.ItemClass.GetItemName()} | currentCount {currentCount} == requiredCount {requiredCount}");

            return currentCount;
        }

        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog(
                $"RemoveRemainingForUpgrade | Trying to remove {itemValue.ItemClass.GetItemName()} | currentCount {currentCount} | requiredCount {requiredCount}");

        return currentCount + RemoveRemaining(itemValue, requiredCount - currentCount);
    }

    public static int RemoveRemainingForRepair(int currentCount, ItemStack itemStack) {
        var num = itemStack.count - currentCount;
        if (num == 0) {
            if (BeyondStorage.Config.isDebug)
                LogUtil.DebugLog(
                    $"RemoveRemainingForRepair | skipping itemStack {itemStack.itemValue.ItemClass.GetItemName()} | stillNeeded {num} == 0");

            return currentCount;
        }

        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog(
                $"RemoveRemainingForRepair | currentCount {currentCount} | itemStack {itemStack.itemValue.ItemClass.GetItemName()} | count {itemStack.count} | stillNeeded {num}");

        return currentCount + RemoveRemaining(itemStack.itemValue, num);
    }

    public static int RemoveRemainingForCraft(int currentCount, ItemValue itemValue, int requiredAmount,
        bool ignoreModdedItems = false,
        IList<ItemStack> removedItems = null) {
        var num = requiredAmount - currentCount;
        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog(
                $"RemoveRemainingForCraft | stillNeeded: {num} current: {currentCount} item: {itemValue.ItemClass.GetItemName()} required: {requiredAmount} ignoreModded: {ignoreModdedItems}");
        //removedCount: {removedItems!.Count}
        if (num <= 0) return currentCount;

        return currentCount - RemoveRemaining(itemValue, num, ignoreModdedItems, removedItems);
    }

    public static int RemoveRemainingForReload(ItemValue ammo, int requiredAmount) {
        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog(
                $"RemoveRemainingForReload | ammo: {ammo.ItemClass.GetItemName()} required: {requiredAmount}");

        return RemoveRemaining(ammo, requiredAmount);
    }

    private static int RemoveRemaining(ItemValue itemValue, int requiredAmount, bool ignoreModdedItems = false,
        IList<ItemStack> removedItems = null) {
        var num = requiredAmount;
        if (BeyondStorage.Config.isDebug)
            LogUtil.DebugLog($"Trying to remove {requiredAmount} {itemValue.ItemClass.GetItemName()}");

        if (requiredAmount <= 0) return requiredAmount;

        ReloadStorages();
        if (_currentStorageDict.Count == 0) return requiredAmount;

        foreach (var kvp in _currentStorageDict) {
            var items = kvp.Value.items;
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
                        LogUtil.DebugLog($"Loc: {kvp.Key}, Value: {kvp.Value}");
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

                    kvp.Value.SetModified();
                } else {
                    removedItems?.Add(items[index].Clone());
                    items[index].Clear();
                    --requiredAmount;
                    kvp.Value.SetModified();
                }
            }
        }

        var result = num - requiredAmount;
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"Removed {result} {itemValue.ItemClass.GetItemName()}");

        return result;
    }

    private static void ReloadStorages() {
        _currentStorageDict.Clear();
        var pos = GameManager.Instance.World.GetPrimaryPlayer().position;
        for (var i = 0; i < GameManager.Instance.World.ChunkClusters.Count; i++) {
            var cc = GameManager.Instance.World.ChunkClusters[i];
            var sync = (ReaderWriterLockSlim)AccessTools.Field(typeof(WorldChunkCache), "sync").GetValue(cc);
            sync.EnterReadLock();
            foreach (var c in cc.chunks.dict.Values) {
                var entities =
                    (DictionaryList<Vector3i, TileEntity>)AccessTools.Field(typeof(Chunk), "tileEntities")
                        .GetValue(c);
                foreach (var kvp in entities.dict) {
                    var loc = kvp.Value.ToWorldPos();
                    if (kvp.Value.IsUserAccessing()) continue;

                    if (!kvp.Value.TryGetSelfOrFeature(out ITileEntityLootable tileEntityLootable)) continue;

                    if (BeyondStorage.Config.onlyStorageCrates)
                        if (!kvp.Value.TryGetSelfOrFeature(out TEFeatureStorage _))
                            continue;

                    if (!tileEntityLootable.bPlayerStorage) continue;

                    if (tileEntityLootable is ILockable tileLockable)
                        if (tileLockable.IsLocked() &&
                            !tileLockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                            continue;

                    if (BeyondStorage.Config.range <= 0 ||
                        Vector3.Distance(pos, loc) < BeyondStorage.Config.range)
                        _currentStorageDict[loc] = tileEntityLootable;
                }
            }

            sync.ExitReadLock();
        }
    }
}
