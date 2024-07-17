using BeyondStorage.Scripts.Common;
using UnityEngine;

namespace BeyondStorage.Scripts;

public static class RangedUtil {
    public static int GetAmmoCountToReload(StateMachineBehaviour animState, ItemValue ammo, int modifiedMagazineSize, int amountReloaded) {
        // Return if no setup to reload from containers
        if (!BeyondStorage.Config.enableForReload) return amountReloaded;

        if (BeyondStorage.Config.isDebug) {
            LogUtil.DebugLog($"Orig Result: {amountReloaded}");
        }

        // capture previously reloaded amount in ammo count
        var currentAmmoCount = amountReloaded;
        // Get the ranged action data and current gun ammo count
        ItemActionRanged actionRanged;
        switch (animState) {
            case AnimatorRangedReloadState animRangedReloadState:
                actionRanged = animRangedReloadState.actionRanged;
                currentAmmoCount += animRangedReloadState.actionData.invData.itemValue.Meta;
                break;
            case Animator3PRangedReloadState animator3PRangedReloadState:
                actionRanged = animator3PRangedReloadState.actionRanged;
                currentAmmoCount += animator3PRangedReloadState.actionData.invData.itemValue.Meta;
                break;
            default:
                return amountReloaded;
        }

        ;

        // Return if we're already max ammo before reloading from nearby containers
        if (currentAmmoCount == modifiedMagazineSize)
            return amountReloaded;

        // newResult = (if AmmoPerMag -> (maxAmmo * RemoveRemainingForReload(ammoType, 1)
        //              else RemoveRemainingForReload(ammoType, maxAmmo - CurrentAmmoCount(meta))
        var reloadCount = actionRanged.AmmoIsPerMagazine
            ? modifiedMagazineSize * ContainerUtils.RemoveRemainingForReload(ammo, 1)
            : ContainerUtils.RemoveRemainingForReload(ammo,
                modifiedMagazineSize - currentAmmoCount);


        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"Additionally Reloaded: {reloadCount}");
        return reloadCount;
    }
}