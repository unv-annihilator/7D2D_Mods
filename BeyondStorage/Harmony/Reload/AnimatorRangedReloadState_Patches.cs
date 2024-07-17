using BeyondStorage.Scripts;
using HarmonyLib;

namespace BeyondStorage.Reload;

[HarmonyPatch(typeof(AnimatorRangedReloadState))]
public class AnimatorRangedReloadStatePatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AnimatorRangedReloadState.GetAmmoCountToReload))]
    private static void AnimatorRangedReloadState_GetAmmoCountToReload_Postfix(AnimatorRangedReloadState __instance,
        ItemValue ammo, int modifiedMagazineSize, ref int __result) {
        var reloadedCount = RangedUtil.GetAmmoCountToReload(__instance, ammo, modifiedMagazineSize, __result);
        __result += reloadedCount;
    }
}