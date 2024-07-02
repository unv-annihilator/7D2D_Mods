using System.Collections.Generic;
using BeyondStorage.Scripts;
using HarmonyLib;

// ReSharper disable InconsistentNaming


namespace BeyondStorage._Bag;

[HarmonyPatch(typeof(Bag))]
public static class Bag_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Bag.GetItemCount))]
    // [HarmonyDebug]
    public static void Bag_GetItemCount_Postfix(ref int __result, ItemValue _itemValue)
    {
        // LogUtil.DebugLog($"Bag GetItemCount | initial result: {__result}");
        __result = ContainerUtils.GetTrueItemCount(_itemValue, __result);
        // LogUtil.DebugLog($"Bag GetItemCount | updated result: {__result}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Bag.DecItem))]
    // [HarmonyDebug]
    public static void Bag_DecItem_Postfix(ref int __result, ItemValue _itemValue, int _count, bool _ignoreModdedItems,
        IList<ItemStack> _removedItems)
    {
        LogUtil.DebugLog($"Bag DecItem | Count: {_count} | Result: {__result}");
        if (_count > 0)
            __result = ContainerUtils.RemoveRemaining(_itemValue, _count, _ignoreModdedItems, _removedItems);
        LogUtil.DebugLog($"Bag DecItem | New Result: {__result}");
    }
}