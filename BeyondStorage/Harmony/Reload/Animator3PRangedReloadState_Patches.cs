using BeyondStorage.Scripts;
using HarmonyLib;

namespace BeyondStorage.Reload;

[HarmonyPatch(typeof(Animator3PRangedReloadState))]
public class Animator3PRangedReloadStatePatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Animator3PRangedReloadState.GetAmmoCountToReload))]
    private static void Animator3PRangedReloadState_GetAmmoCountToReload_Postfix(AnimatorRangedReloadState __instance,
        ItemValue ammo, int modifiedMagazineSize, ref int __result) {
        var reloadedCount = RangedUtil.GetAmmoCountToReload(__instance, ammo, modifiedMagazineSize, __result);
        __result += reloadedCount;
    }
}