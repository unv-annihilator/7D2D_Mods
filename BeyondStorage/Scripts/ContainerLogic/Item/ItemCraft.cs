﻿using System.Collections.Generic;
using BeyondStorage.Scripts.Common;

namespace BeyondStorage.Scripts.ContainerLogic.Item;

public class ItemCraft {
    internal static bool IngredientListShown { get; set; }

    // Used By:
    //      XUiC_RecipeCraftCount.calcMaxCraftable
    //          Item Crafting - gets max craftable amount
    public static List<ItemStack> ItemCraftMaxGetAllStorageStacks(List<ItemStack> items) {
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"ItemCraftMaxGetAllStorageStacks | itemCount before {items.Count}");
        items.AddRange(ContainerUtils.GetItemStacks());
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"ItemCraftMaxGetAllStorageStacks | itemCount after {items.Count}");
        return items;
    }

    // Used By:
    //      XUiC_RecipeList.Update
    //          Item Crafts - shown as available in the list
    public static void ItemCraftGetAllStorageStacks(List<ItemStack> items) {
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"ItemCraftGetAllStorageStacks | items.Count before {items.Count}");
        items.AddRange(ContainerUtils.GetItemStacks());
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"ItemCraftGetAllStorageStacks | items.Count after {items.Count}");
    }

    //  Used By:
    //      XUiC_IngredientEntry.GetBindingValue
    //          Item Crafting - shows item count available in crafting window(s)
    public static int EntryBindingAddAllStorageCount(int count, XUiC_IngredientEntry entry) {
        var itemValue = entry.Ingredient.itemValue;
        var storageCount = ContainerUtils.GetItemCount(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"EntryBindingAddAllStorageCount | item {itemValue.ItemClass.GetItemName()}; initialCount {count}; storageCount {storageCount}");
        return count + storageCount;
    }

    // Used By:
    //      XUiM_PlayerInventory.HasItems
    //          Item Crafting -
    public static int HasItemGetItemCount(ItemValue itemValue) {
        var storageCount = ContainerUtils.GetItemCount(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"HasItemGetItemCount | item {itemValue.ItemClass.GetItemName()}; storageCount {storageCount}");
        return storageCount;
    }
}