using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace BeyondStorage.Scripts;

public static class ContainerUtils
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static Dictionary<Vector3i, ITileEntityLootable> _currentStorageDict = new();

    public static void Init()
    {
        _currentStorageDict.Clear();
    }

    public static void AddAllStorageStacks(List<ItemStack> items)
    {
        ReloadStorages();
        if (_currentStorageDict.Count == 0)
            return;
        foreach (var kvp in _currentStorageDict) items.AddRange(kvp.Value.items);
    }

    public static List<ItemStack> GetAllStorageStacks(List<ItemStack> items)
    {
        ReloadStorages();
        if (_currentStorageDict.Count == 0)
            return items;
        var itemList = new List<ItemStack>();
        itemList.AddRange(items);
        foreach (var kvp in _currentStorageDict) itemList.AddRange(kvp.Value.items);
        return itemList;
    }

    private static int GetItemCount(ItemStack[] slots, ItemValue itemValue)
    {
        return slots.Where(t => t.itemValue.type == itemValue.type).Sum(t => t.count);
    }

    public static int GetTrueItemCount(ItemValue itemValue, int currentCount)
    {
        ReloadStorages();
        if (_currentStorageDict.Count == 0)
            return currentCount;
        return currentCount + _currentStorageDict.Select(kvp => kvp.Value.items)
            .Sum(items => GetItemCount(items, itemValue));
    }

    public static int RemoveRemaining(ItemValue itemValue, int requiredAmount, bool ignoreModdedItems,
        IList<ItemStack> removedItems)
    {
        var num = requiredAmount;
        LogUtil.DebugLog($"Trying to remove {requiredAmount} {itemValue.ItemClass.GetItemName()}");
        if (requiredAmount <= 0)
            return requiredAmount;
        ReloadStorages();
        if (_currentStorageDict.Count == 0)
            return requiredAmount;
        foreach (var kvp in _currentStorageDict)
        {
            var items = kvp.Value.items;
            for (var index = 0; requiredAmount > 0 && index < items.Length; ++index)
                if (items[index].itemValue.type == itemValue.type && (!ignoreModdedItems ||
                                                                      !items[index].itemValue.HasModSlots ||
                                                                      !items[index].itemValue.HasMods()))
                {
                    if (ItemClass.GetForId(items[index].itemValue.type).CanStack())
                    {
                        LogUtil.DebugLog($"Loc: {kvp.Key}, Value: {kvp.Value}");
                        var itemCount = items[index].count;
                        var countToRemove = itemCount >= requiredAmount ? requiredAmount : itemCount;
                        LogUtil.DebugLog($"Item Count: {itemCount} Count To Remove: {countToRemove}");
                        removedItems?.Add(new ItemStack(items[index].itemValue.Clone(), countToRemove));
                        LogUtil.DebugLog($"Item Count Before: {items[index].count}");
                        items[index].count -= countToRemove;
                        LogUtil.DebugLog($"Item Count After: {items[index].count}");
                        requiredAmount -= countToRemove;
                        LogUtil.DebugLog($"Required After: {requiredAmount}");
                        if (items[index].count <= 0) items[index].Clear();
                        kvp.Value.SetModified();
                    }
                    else
                    {
                        removedItems?.Add(items[index].Clone());
                        items[index].Clear();
                        --requiredAmount;
                        kvp.Value.SetModified();
                    }
                }
        }

        LogUtil.DebugLog($"Removed {num - requiredAmount} {itemValue.ItemClass.GetItemName()}");
        return num - requiredAmount;
    }

    private static void ReloadStorages()
    {
        _currentStorageDict.Clear();
        var pos = GameManager.Instance.World.GetPrimaryPlayer().position;
        for (var i = 0; i < GameManager.Instance.World.ChunkClusters.Count; i++)
        {
            var cc = GameManager.Instance.World.ChunkClusters[i];
            var sync = (ReaderWriterLockSlim)AccessTools.Field(typeof(WorldChunkCache), "sync").GetValue(cc);
            sync.EnterReadLock();
            foreach (var c in cc.chunks.dict.Values)
            {
                var entities =
                    (DictionaryList<Vector3i, TileEntity>)AccessTools.Field(typeof(Chunk), "tileEntities")
                        .GetValue(c);
                foreach (var kvp in entities.dict)
                {
                    var loc = kvp.Value.ToWorldPos();
                    if (kvp.Value.IsUserAccessing())
                        continue;
                    if (!kvp.Value.TryGetSelfOrFeature(out ITileEntityLootable tileEntityLootable))
                        continue;
                    if (!tileEntityLootable.bPlayerStorage)
                        continue;
                    if (tileEntityLootable is ILockable tileLockable)
                        if (tileLockable.IsLocked() &&
                            tileLockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                            continue;
                    // KnownStorageDict[loc] = tileEntityLootable;
                    // LogUtil.DebugLog(
                    // $"Config Range: {BeyondStorage.Config.range} | Distance: {Vector3.Distance(pos, loc)}");
                    if (BeyondStorage.Config.range <= 0 ||
                        Vector3.Distance(pos, loc) < BeyondStorage.Config.range)
                        _currentStorageDict[loc] = tileEntityLootable;
                }
            }

            sync.ExitReadLock();
        }
    }
}
