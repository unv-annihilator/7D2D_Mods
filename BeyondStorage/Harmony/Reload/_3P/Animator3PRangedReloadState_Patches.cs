using System.Collections.Generic;
using BeyondStorage.Scripts.Configuration;
using HarmonyLib;

namespace BeyondStorage.Reload._3P;

[HarmonyPatch(typeof(Animator3PRangedReloadState))]
public class Animator3PRangedReloadStatePatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Animator3PRangedReloadState.GetAmmoCountToReload))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> Animator3PRangedReloadState_GetAmmoCountToReload_Patch(IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(Animator3PRangedReloadState)}.{nameof(Animator3PRangedReloadState.GetAmmoCountToReload)}";
        return AnimatorCommon.GetCountToReload_Transpiler(targetMethodString, instructions);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Animator3PRangedReloadState.GetAmmoCount))]
    public static void Animator3PRangedReloadState_GetAmmoCount_Postfix(ref int __result, ItemValue ammo, int modifiedMagazineSize) {
        if (!ModConfig.EnableForReload()) return;
        __result = AnimatorCommon.GetAmmoCount(ammo, __result, modifiedMagazineSize);
    }
}