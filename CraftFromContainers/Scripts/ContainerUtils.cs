using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace CraftFromContainers.Scripts
{
    public static class ContainerUtils
    {
        private static Dictionary<Vector3i, ITileEntityLootable> _knownStorageDict =
            new Dictionary<Vector3i, ITileEntityLootable>();

        private static Dictionary<Vector3i, ITileEntityLootable> currentStorageDict =
            new Dictionary<Vector3i, ITileEntityLootable>();

        public static void Init()
        {
            _knownStorageDict.Clear();
        }

        public static void AddAllStorageStacks(List<ItemStack> items)
        {
            ReloadStorages();
            if (currentStorageDict.Count == 0)
                return;
            foreach (var kvp in currentStorageDict) items.AddRange(kvp.Value.items);
        }

        public static int AddAllStoragesCountItem(int count, ItemValue item)
        {
            ReloadStorages();
            if (currentStorageDict.Count == 0)
                return count;
            count += currentStorageDict.Select(kvp => kvp.Value.items).Sum(items =>
                items.Where(t => t.itemValue.type == item.type).Sum(t => t.count));
            return count;
        }

        public static int AddAllStoragesCountEntry(int count, XUiC_IngredientEntry entry)
        {
            return AddAllStoragesCountItem(count, entry.Ingredient.itemValue);
        }

        public static int AddAllStoragesCountItemStack(int count, ItemStack itemStack)
        {
            return AddAllStoragesCountItem(count, itemStack.itemValue);
        }

        public static ItemStack[] GetAllStorageStacks(ItemStack[] items)
        {
            ReloadStorages();
            if (currentStorageDict.Count == 0)
                return items;
            var itemList = new List<ItemStack>();
            itemList.AddRange(items);
            foreach (var kvp in currentStorageDict) itemList.AddRange(kvp.Value.items);
            return itemList.ToArray();
        }

        private static int GetItemCount(ItemStack[] slots, ItemValue itemValue)
        {
            return slots.Where(t => t.itemValue.type == itemValue.type).Sum(t => t.count);
        }

        public static int GetTrueRemaining(IList<ItemStack> itemStacks, int i, int numLeft)
        {
            ReloadStorages();
            if (currentStorageDict.Count == 0)
                return numLeft;
            foreach (var items in currentStorageDict.Select(kvp => kvp.Value.items))
            {
                numLeft -= GetItemCount(items, itemStacks[i].itemValue);
                if (numLeft <= 0)
                    return numLeft;
            }

            return numLeft;
        }

        public static List<ItemStack> GetAllStorageStacks2(List<ItemStack> items)
        {
            ReloadStorages();
            if (currentStorageDict.Count == 0)
                return items;
            var itemList = new List<ItemStack>();
            itemList.AddRange(items);
            foreach (var kvp in currentStorageDict) itemList.AddRange(kvp.Value.items);
            return itemList;
        }

        public static void RemoveRemainingForCraft(IList<ItemStack> itemStacks, int i, int numLeft)
        {
            ReloadStorages();
            if (currentStorageDict.Count == 0)
                return;
            LogUtil.DebugLog($"Trying to remove {numLeft} {itemStacks[i].itemValue.ItemClass.GetItemName()}");
            foreach (var kvp in currentStorageDict)
            {
                var items = kvp.Value.items;
                foreach (var t in items)
                {
                    if (t.itemValue.type != itemStacks[i].itemValue.type) continue;

                    var toRem = Math.Min(numLeft, t.count);
                    LogUtil.DebugLog($"Removing {toRem}/{numLeft} {itemStacks[i].itemValue.ItemClass.GetItemName()}");
                    numLeft -= toRem;
                    if (t.count <= toRem)
                        t.Clear();
                    else
                        t.count -= toRem;

                    kvp.Value.SetModified();
                    if (numLeft <= 0)
                        return;
                }
            }
        }

        public static int RemoveRemainingForUpgrade(int numRemoved, ItemActionRepair action, Block block)
        {
            if (!CraftFromContainers.Config.modEnabled)
                return numRemoved;
            if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out var totalToRemove))
                return numRemoved;
            var itemValue = ItemClass.GetItem(action.GetUpgradeItemName(block));
            if (totalToRemove <= numRemoved)
                return numRemoved;

            var numLeft = totalToRemove - numRemoved;

            ReloadStorages();

            if (currentStorageDict.Count == 0)
                return numRemoved;

            foreach (var kvp in currentStorageDict)
            {
                var items = kvp.Value.items;
                foreach (var t in items)
                    if (t.itemValue.type == itemValue.type)
                    {
                        var toRem = Math.Min(numLeft, t.count);
                        numLeft -= toRem;
                        if (t.count <= toRem)
                            t.Clear();
                        else
                            t.count -= toRem;

                        kvp.Value.SetModified();
                        if (numLeft <= 0)
                            return totalToRemove;
                    }
            }

            return totalToRemove - numLeft;
        }

        public static int RemoveRemainingForRepair(int numRemoved, ItemStack _itemStack)
        {
            if (!CraftFromContainers.Config.modEnabled)
                return numRemoved;
            var totalToRemove = _itemStack.count;

            if (totalToRemove <= numRemoved)
                return numRemoved;

            var numLeft = totalToRemove - numRemoved;

            ReloadStorages();

            if (currentStorageDict.Count == 0)
                return numRemoved;

            foreach (var kvp in currentStorageDict)
            {
                var items = kvp.Value.items;
                foreach (var t in items)
                    if (t.itemValue.type == _itemStack.itemValue.type)
                    {
                        var toRem = Math.Min(numLeft, t.count);
                        numLeft -= toRem;
                        if (t.count <= toRem)
                            t.Clear();
                        else
                            t.count -= toRem;

                        kvp.Value.SetModified();
                        if (numLeft <= 0)
                            return totalToRemove;
                    }
            }

            return totalToRemove - numLeft;
        }

        private static void ReloadStorages()
        {
            currentStorageDict.Clear();
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
                        {
                            if (tileLockable.IsLocked() &&
                                tileLockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                                continue;
                        }
                        _knownStorageDict[loc] = tileEntityLootable;
                        if (CraftFromContainers.Config.range <= 0 ||
                            Vector3.Distance(pos, loc) < CraftFromContainers.Config.range)
                            currentStorageDict[loc] = tileEntityLootable;
                    }
                }
                sync.ExitReadLock();
            }
        }
    }
}