using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BeyondStorage.Scripts.ContainerLogic.Ranged;
using BeyondStorage.Scripts.Utils;
using HarmonyLib;
using UnityEngine;

namespace BeyondStorage.Reload;

public static class AnimatorCommon {
    public static int GetAmmoCount(ItemValue ammoType, int lastResult, int maxAmmo) {
        return maxAmmo == lastResult ? lastResult : Mathf.Min(Ranged.GetAmmoCount(ammoType) + lastResult, maxAmmo);
    }

    internal static IEnumerable<CodeInstruction> GetCountToReload_Transpiler(string targetMethodString, IEnumerable<CodeInstruction> instructions) {
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codeInstructions = new List<CodeInstruction>(instructions);
        var lastRet = codeInstructions.FindLastIndex(codeInstruction => codeInstruction.opcode == OpCodes.Ret);
        if (lastRet != -1) {
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Found last ret at {lastRet} for {targetMethodString}");
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
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Ranged), nameof(Ranged.RemoveAmmoForReload)))
            ];

            // insert before last ret
            codeInstructions.InsertRange(lastRet, newCode);
            LogUtil.Info($"Successfully patched {targetMethodString}");
        } else {
            LogUtil.Error($"Failed to patch {targetMethodString}");
        }

        return codeInstructions.AsEnumerable();
    }
}