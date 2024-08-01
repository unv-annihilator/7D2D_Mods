using System.Collections.Generic;
using BeyondStorage.Scripts.Configuration;
using HarmonyLib;

namespace BeyondStorage.Reload.FP;

[HarmonyPatch(typeof(AnimatorRangedReloadState))]
public class AnimatorRangedReloadStatePatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(AnimatorRangedReloadState.GetAmmoCountToReload))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> AnimatorRangedReloadState_GetAmmoCountToReload_Patch(IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(AnimatorRangedReloadState)}.{nameof(AnimatorRangedReloadState.GetAmmoCountToReload)}";
        return AnimatorCommon.GetCountToReload_Transpiler(targetMethodString, instructions);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AnimatorRangedReloadState.GetAmmoCount))]
    public static void AnimatorRangedReloadState_GetAmmoCount_Postfix(ref int __result, ItemValue ammo, int modifiedMagazineSize) {
        if (!ModConfig.EnableForReload()) return;
        __result = AnimatorCommon.GetAmmoCount(ammo, __result, modifiedMagazineSize);
    }
}