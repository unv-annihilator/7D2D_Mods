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

    internal static bool IngredientListShown { get; set; }

    internal static bool RepairActionShown { get; set; }

    internal static bool ActionListVisible { get; set; }

    public static void Init() {
        _currentStorageDict.Clear();
    }

    public static void Cleanup() {
        _currentStorageDict.Clear();
        IngredientListShown = false;
        RepairActionShown = false;
        ActionListVisible = false;
    }

    private static bool ShouldCheck(bool forRepair = false) {
        return IngredientListShown || forRepair && BeyondStorage.Config.enableForItemRepair && RepairActionShown && ActionListVisible;
    }


    //  Used By:
    //      XUiC_IngredientEntry_GetBindingValue_Patch
    // Used For:
    //      Item Crafting (shows item count available in crafting window(s))
    //      Item Repair (removes items with Bag DecItem)
    public static int AddAllStoragesCountEntry(int count, XUiC_IngredientEntry entry) {
        if (LogUtil.IsDebugEnabled())
            LogUtil.DebugLog(
                $"AddAllStoragesCountEntry | count {count} |  entry {entry.Ingredient.itemValue.ItemClass.GetItemName()}");

        return GetItemCount2(count, entry.Ingredient.itemValue);
    }

    //  Used By:
    //      XUiC_IngredientEntry_GetBindingValue_Patch
    // Used For:
    //      Item Crafting (shows item count available in crafting window(s))
    //      Item Repair (removes items with Bag DecItem)
    private static int GetItemCount2(int count, ItemValue item) {
        ReloadStorages();
        if (_currentStorageDict.Count == 0) return count;

        count += _currentStorageDict.Select(kvp => kvp.Value.items)
            .Sum(items => items.Where(t => t.itemValue.type == item.type).Sum(t => t.count));
        return count;
    }

    // Used By:
    //      XUiC_RecipeCraftCount.calcMaxCraftable (Item Crafting (gets max craftable amount))
    public static ItemStack[] GetAllStorageStacksArrays(ItemStack[] items) {
        var tempList = items.ToList();
        AddAllStorageStacks(tempList);
        return tempList.ToArray();
    }

    // Used By:
    //      XUiC_RecipeList.Update (Item Crafts (shown as available in the list))
    //      XUiC_RecipeCraftCount.calcMaxCraftable (Item Crafting (gets max craftable amount))
    public static void AddAllStorageStacks(List<ItemStack> items) {
        ReloadStorages();
        if (_currentStorageDict.Count == 0) return;

        foreach (var kvp in _currentStorageDict) {
            items.AddRange(kvp.Value.items);
        }
    }

    // Used By:
    //      ItemActionEntryRepair.RefreshEnabled (Item Repair (Button Enabled))
    public static int EntryRefresh_GetItemCount(ItemValue itemValue) {
#if DEBUG
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"EntryRefresh_GetItemCount | item {itemValue.ItemClass.GetItemName()}; shouldCheck {ShouldCheck(true)}");
#endif
        if (ShouldCheck(true))
            return GetItemCount(itemValue);
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog("EntryRefresh_GetItemCount Blocked due to shouldn't check");
        return 0;
    }

    // Used By:
    //      XUiM_PlayerInventory.HasItems (Item Crafting (has items only, does not handle remove))
    public static int GetItemCountForItem(ItemValue itemValue) {
#if DEBUG
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"GetItemCountForItem | item {itemValue.ItemClass.GetItemName()}");
#endif
        return GetItemCount(itemValue);
    }

    // Used By:
    //      ItemActionRepair.CanRemoveRequiredResource (Block Upgrade (Check for enough items))
    //      ItemActionRepair.canRemoveRequiredItem (Block Repair (resources available check))
    //      ItemActionEntryRepair.OnActivated (Item Repair (Allows Repair)) -- Already skips if not in inventory
    public static int GetItemCount(ItemValue itemValue) {
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"GetItemCount | item {itemValue.ItemClass.GetItemName()}");

        ReloadStorages();
        return _currentStorageDict.Count == 0
            ? 0
            : _currentStorageDict.Select(kvp => kvp.Value.items).Sum(items => GetItemCountFromSlots(items, itemValue));
    }

    private static int GetItemCountFromSlots(ItemStack[] slots, ItemValue itemValue) {
        return slots.Where(t => t.itemValue.type == itemValue.type).Sum(t => t.count);
    }


    // Used By:
    //      ItemActionRanged.CanReload (Weapon Reload - Ammo Exists Check)
    public static bool HasItem(ItemValue itemValue) {
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"HasItem | item {itemValue.ItemClass.GetItemName()}");
        ReloadStorages();
        return _currentStorageDict.Select(kvp => kvp.Value.items).Any(items => items.Any(itemStack => itemStack.itemValue.type == itemValue.type));
    }

    // Used By:
    //      ItemActionEntryRepair.OnActivated (Item Repair (Allows Repair))
    public static int GetTrueItemRepairCount(ItemValue itemValue, int currentCount) {
        if (LogUtil.IsDebugEnabled())
            LogUtil.DebugLog($"GetTrueItemRepairCount | item {itemValue.ItemClass.GetItemName()} | currentCount {currentCount}");

        // Skip if we already have enough in inventory
        if (currentCount * itemValue.ItemClass.RepairAmount.Value <= 0) {
            if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog("GetTrueItemRepairCount | adding nearby inventory");

            return currentCount + GetItemCount(itemValue);
        }

        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog("GetTrueItemRepairCount | returning current count");

        return currentCount;
    }

    // Used By:
    //      ItemActionRepair.RemoveRequiredResource (Block Upgrade (Remove items))
    public static int RemoveRemainingForUpgrade(int currentCount, ItemValue itemValue, int requiredCount) {
        if (currentCount == requiredCount) {
            if (LogUtil.IsDebugEnabled())
                LogUtil.DebugLog(
                    $"RemoveRemainingForUpgrade | Skipping {itemValue.ItemClass.GetItemName()} | currentCount {currentCount} == requiredCount {requiredCount}");

            return currentCount;
        }

        if (LogUtil.IsDebugEnabled())
            LogUtil.DebugLog(
                $"RemoveRemainingForUpgrade | Trying to remove {itemValue.ItemClass.GetItemName()} | currentCount {currentCount} | requiredCount {requiredCount}");

        return currentCount + RemoveRemaining(itemValue, requiredCount - currentCount);
    }

    // Used By:
    //      ItemActionRepair.removeRequiredItem (Block Repair (remove items on repair))
    public static int RemoveRemainingForRepair(int currentCount, ItemStack itemStack) {
        var num = itemStack.count - currentCount;
        if (num == 0) {
            if (LogUtil.IsDebugEnabled())
                LogUtil.DebugLog(
                    $"RemoveRemainingForRepair | skipping itemStack {itemStack.itemValue.ItemClass.GetItemName()} | stillNeeded {num} == 0");

            return currentCount;
        }

        if (LogUtil.IsDebugEnabled())
            LogUtil.DebugLog(
                $"RemoveRemainingForRepair | currentCount {currentCount} | itemStack {itemStack.itemValue.ItemClass.GetItemName()} | count {itemStack.count} | stillNeeded {num}");

        return currentCount + RemoveRemaining(itemStack.itemValue, num);
    }

    // Used By:
    //      XUiM_PlayerInventory.RemoveItems
    // Used For:
    //      Item Crafting (Remove items on craft)
    //      Item Repair (Remove items on repair)
    public static int RemoveRemainingForCraft(int currentCount, ItemValue itemValue, int requiredAmount,
        bool ignoreModdedItems = false,
        IList<ItemStack> removedItems = null) {
        var num = requiredAmount - currentCount;
        if (LogUtil.IsDebugEnabled())
            LogUtil.DebugLog(
                $"RemoveRemainingForCraft | stillNeeded: {num} current: {currentCount} item: {itemValue.ItemClass.GetItemName()} required: {requiredAmount} ignoreModded: {ignoreModdedItems}");
        if (num <= 0) return currentCount;

        return currentCount - RemoveRemaining(itemValue, num, ignoreModdedItems, removedItems);
    }

    public static int RemoveRemaining(ItemValue itemValue, int requiredAmount, bool ignoreModdedItems = false,
        IList<ItemStack> removedItems = null) {
        var num = requiredAmount;
        if (LogUtil.IsDebugEnabled())
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
                    if (LogUtil.IsDebugEnabled()) {
                        LogUtil.DebugLog($"Loc: {kvp.Key}, Value: {kvp.Value}");
                        LogUtil.DebugLog($"Item Count: {itemCount} Count To Remove: {countToRemove}");
                        LogUtil.DebugLog($"Item Count Before: {items[index].count}");
                    }

                    removedItems?.Add(new ItemStack(items[index].itemValue.Clone(), countToRemove));
                    items[index].count -= countToRemove;
                    requiredAmount -= countToRemove;
                    if (LogUtil.IsDebugEnabled()) {
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
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"Removed {result} {itemValue.ItemClass.GetItemName()}");

        return result;
    }

    private static void ReloadStorages() {
        _currentStorageDict.Clear();
        var pos = GameManager.Instance.World.GetPrimaryPlayer().position;
        for (var i = 0; i < GameManager.Instance.World.ChunkClusters.Count; i++) {
            var cc = GameManager.Instance.World.ChunkClusters[i];
            var sync = (ReaderWriterLockSlim)AccessTools.Field(typeof(WorldChunkCache), "sync").GetValue(cc);
            sync.EnterReadLock();
            foreach (var kvp in cc.chunks.dict.Values.Select(c => (DictionaryList<Vector3i, TileEntity>)AccessTools.Field(typeof(Chunk), "tileEntities")
                         .GetValue(c)).SelectMany(entities => entities.dict)) {
                if (!kvp.Value.TryGetSelfOrFeature(out ITileEntityLootable tileEntityLootable)) continue;

                if (BeyondStorage.Config.onlyStorageCrates)
                    if (!kvp.Value.TryGetSelfOrFeature(out TEFeatureStorage _))
                        continue;

                if (!tileEntityLootable.bPlayerStorage) continue;

#if DEBUG
                LogUtil.DebugLog($"{kvp.Key} {kvp.Value.IsUserAccessing()}");
#endif

                if (kvp.Value.IsUserAccessing()) continue;

                if (kvp.Value.TryGetSelfOrFeature(out ILockable tileLockable))
                    if (tileLockable.IsLocked() && !tileLockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                        continue;

                var loc = kvp.Value.ToWorldPos();
                if (BeyondStorage.Config.range <= 0 ||
                    Vector3.Distance(pos, loc) < BeyondStorage.Config.range)
                    _currentStorageDict[loc] = tileEntityLootable;
            }

            sync.ExitReadLock();
        }
    }
}