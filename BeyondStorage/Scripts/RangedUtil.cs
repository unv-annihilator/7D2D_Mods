using BeyondStorage.Scripts.Common;

namespace BeyondStorage.Scripts;

public static class RangedUtil {
    // Used By:
    //      ItemActionRanged.CanReload (Weapon Reload - Ammo Exists Check)
    public static bool CanReloadFromStorage(ItemValue itemValue) {
        // Get ammo count for ammo type; setting __result to (Count > 0)
        var newResult = ContainerUtils.HasItem(itemValue);
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"New Result: {newResult}");

        // Set new result
        return newResult;
    }

    // TODO: Update this to return early if we hit the max ammo for mag
    // Used By:
    //      AnimatorRangedReloadState.GetAmmoCount (Weapon Reload - Get Total Ammo Count (not displayed))
    //      Animator3PRangedReloadState.GetAmmoCount (Weapon Reload - Get Total Ammo Count (not displayed))
    public static int GetAmmoCount(ItemValue itemValue) {
        return ContainerUtils.GetItemCount(itemValue);
        // return Container2Utils.GetAvailableStorages().Sum(tileEntityLootable => tileEntityLootable.items.Where(t => t.itemValue.type == itemValue.type).Sum(t => t.count));
    }

    // Used By:
    //      AnimatorRangedReloadState.GetAmmoCountToReload (Weapon Reload - Remove Items For Reload)
    //      Animator3PRangedReloadState.GetAmmoCountToReload (Weapon Reload - Remove Items For Reload)
    public static int RemoveAmmoForReload(ItemValue ammoType, bool isPerMag, int maxMagSize, int currentAmmo) {
        var ammoRequired = isPerMag ? 1 : maxMagSize - currentAmmo;
        var ammoRemovedFromStorage = ContainerUtils.RemoveRemaining(ammoType, ammoRequired);
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"{ammoType.ItemClass.GetItemName()} {isPerMag} {maxMagSize} {currentAmmo} {ammoRemovedFromStorage}");
        return isPerMag ? maxMagSize * ammoRemovedFromStorage : ammoRemovedFromStorage;
    }
}