using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.ContainerLogic.Ranged;

public static class Ranged {
    // Used By:
    //      ItemActionRanged.CanReload (Weapon Reload - Ammo Exists Check)
    public static bool CanReloadFromStorage(ItemValue itemValue) {
        // return 0 if not enabled for reloading
        if (!ModConfig.EnableForReload()) return false;
        // otherwise look for ammo
        var canReloadFromStorage = ContainerUtils.HasItem(itemValue);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"canReloadFromStorage: {canReloadFromStorage}");
        return canReloadFromStorage;
    }

    // TODO: Update this to return early if we hit the max ammo for mag
    // Used By:
    //      AnimatorRangedReloadState.GetAmmoCount (Weapon Reload - Get Total Ammo Count (not displayed))
    //      Animator3PRangedReloadState.GetAmmoCount (Weapon Reload - Get Total Ammo Count (not displayed))
    public static int GetAmmoCount(ItemValue itemValue) {
        return ContainerUtils.GetItemCount(itemValue);
    }

    // Used By:
    //      AnimatorRangedReloadState.GetAmmoCountToReload (Weapon Reload - Remove Items For Reload)
    //      Animator3PRangedReloadState.GetAmmoCountToReload (Weapon Reload - Remove Items For Reload)
    public static int RemoveAmmoForReload(ItemValue ammoType, bool isPerMag, int maxMagSize, int currentAmmo) {
        // return 0 if not enabled for reloading
        if (!ModConfig.EnableForReload()) return 0;
        var ammoRequired = isPerMag ? 1 : maxMagSize - currentAmmo;
        var ammoRemovedFromStorage = ContainerUtils.RemoveRemaining(ammoType, ammoRequired);
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"{ammoType.ItemClass.GetItemName()} {isPerMag} {maxMagSize} {currentAmmo} {ammoRemovedFromStorage}");
        return isPerMag ? maxMagSize * ammoRemovedFromStorage : ammoRemovedFromStorage;
    }
}