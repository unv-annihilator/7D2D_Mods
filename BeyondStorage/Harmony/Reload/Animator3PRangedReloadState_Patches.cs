using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Reload;

[HarmonyPatch(typeof(Animator3PRangedReloadState))]
public class Animator3PRangedReloadStatePatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Animator3PRangedReloadState.GetAmmoCountToReload))]
    private static void Animator3PRangedReloadState_GetAmmoCountToReload_Postfix(AnimatorRangedReloadState __instance,
        ItemValue ammo, int modifiedMagazineSize, ref int __result) {
        if (!BeyondStorage.Config.enableForReload) return;

        if (__result > 0) return;

        if (BeyondStorage.Config.isDebug) {
            LogUtil.DebugLog("AnimatorRangedReloadState_GetAmmoCountToReload_Postfix");
            // LogUtil.DebugLog($"Bag Ammo Count: {ea.bag.GetItemCount(ammo)}");
            // LogUtil.DebugLog($"Inv Ammo Count: {ea.inventory.GetItemCount(ammo)}");
            LogUtil.DebugLog($"Orig Result: {__result}");
            // LogUtil.DebugLog($"Meta: {__instance.actionData.invData.itemValue.Meta}");
            // LogUtil.DebugLog($"ModifiedMagSize: {modifiedMagazineSize}");
        }

        // newResult = (if AmmoPerMag -> (maxAmmo * RemoveRemainingForReload(ammoType, 1)
        //              else RemoveRemainingForReload(ammoType, maxAmmo - CurrentAmmoCount(meta))
        var newResult = __instance.actionRanged.AmmoIsPerMagazine
            ? modifiedMagazineSize * ContainerUtils.RemoveRemainingForReload(ammo, 1)
            : ContainerUtils.RemoveRemainingForReload(ammo,
                modifiedMagazineSize - __instance.actionData.invData.itemValue.Meta);
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog($"New Result: {newResult}");

        __result = newResult;
    }
}
