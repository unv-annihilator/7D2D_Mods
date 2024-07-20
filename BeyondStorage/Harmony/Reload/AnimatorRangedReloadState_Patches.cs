using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;
using UnityEngine;

namespace BeyondStorage.Reload;

[HarmonyPatch(typeof(AnimatorRangedReloadState))]
public class AnimatorRangedReloadStatePatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(AnimatorRangedReloadState.GetAmmoCountToReload))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> AnimatorRangedReloadState_GetAmmoCountToReload_Patch(IEnumerable<CodeInstruction> instructions) {
        if (!BeyondStorage.Config.enableForReload) return instructions;
        var targetMethodString = $"{typeof(AnimatorRangedReloadState)}.{nameof(AnimatorRangedReloadState.GetAmmoCountToReload)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codeInstructions = new List<CodeInstruction>(instructions);
        var lastRet = codeInstructions.FindLastIndex(codeInstruction => codeInstruction.opcode == OpCodes.Ret);
        if (lastRet != -1) {
            if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"Found last ret at {lastRet} for {targetMethodString}");
            var start = new CodeInstruction(OpCodes.Ldarg_2);
            codeInstructions[lastRet - 1].MoveLabelsTo(start);
            codeInstructions[lastRet - 1] = new CodeInstruction(OpCodes.Nop);
            List<CodeInstruction> newCode = [
                // ldarg.2  // ammo (ItemValue)
                start,
                // this.actionRanged.AmmoIsPerMagazine
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AnimatorRangedReloadState), nameof(AnimatorRangedReloadState.actionRanged))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemActionAttack), nameof(ItemActionAttack.AmmoIsPerMagazine))),
                // modifiedMagazineSize
                new CodeInstruction(OpCodes.Ldarg_3),
                // this.actionData.invData.itemValue.Meta
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AnimatorRangedReloadState), nameof(AnimatorRangedReloadState.actionData))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemActionData), nameof(ItemActionData.invData))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ItemInventoryData), nameof(ItemInventoryData.itemValue))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemValue), nameof(ItemValue.Meta))),
                // RemoveAmmoForReload(ItemValue ammoType, bool isPerMag, int maxMagSize, int currentAmmo)
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RangedUtil), nameof(RangedUtil.RemoveAmmoForReload)))
            ];

            // insert before last ret
            codeInstructions.InsertRange(lastRet, newCode);
            LogUtil.Info($"Successfully patched {targetMethodString}");
        } else {
            LogUtil.Error($"Failed to patch {targetMethodString}");
        }

        return codeInstructions.AsEnumerable();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AnimatorRangedReloadState.GetAmmoCount))]
    public static void AnimatorRangedReloadState_GetAmmoCount_Postfix(ref int __result, ItemValue ammo, int modifiedMagazineSize) {
        if (!BeyondStorage.Config.enableForReload) return;
        if (modifiedMagazineSize == __result) return;
        __result = Mathf.Min(RangedUtil.GetAmmoCount(ammo) + __result, modifiedMagazineSize);
    }
}