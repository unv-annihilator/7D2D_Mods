using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace BeyondStorage.Reload;

[HarmonyPatch(typeof(ItemActionRanged))]
public class ItemActionRangedPatches {
    // Used For:
    //          Weapon Reload (check if allowed to reload)
    // TODO: Maybe make Transpiler to avoid re-grabbing information such as maxAmmo and Ammo required
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemActionRanged.CanReload))]
    private static void ItemActionRanged_CanReload_Postfix(ItemActionRanged __instance, ItemActionData _actionData,
        ref bool __result) {
        // Skip if not enabled in config
        if (!BeyondStorage.Config.enableForReload) return;

        // Skip if we're already allowed to reload
        if (__result) return;

        if (BeyondStorage.Config.isDebug) {
            LogUtil.DebugLog("ItemActionRanged_CanReload_Postfix");
            LogUtil.DebugLog($"Orig Result: {__result}");
        }

        // Convert _actionData
        var actionData = (ItemActionRanged.ItemActionDataRanged)_actionData;
        // Get the held item
        var holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
        // Get the ammo required
        var itemValue = ItemClass.GetItem(__instance.MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex]);
        // Get max mag size
        var num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, holdingItemItemValue,
            __instance.BulletsPerMagazine, _actionData.invData.holdingEntity);
        // Check if we're not already reloading OR aren't already full on ammo
        if (!__instance.notReloading(actionData) || _actionData.invData.itemValue.Meta >= num) return;

        // Get ammo count for ammo type; setting __result to (Count > 0)
        var newResult = ContainerUtils.GetItemCount(itemValue) > 0;
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"New Result: {newResult}");

        // Set new result
        __result = newResult;
    }
}
