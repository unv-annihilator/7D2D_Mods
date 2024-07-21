using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.ContainerLogic.Ranged;
using HarmonyLib;

namespace BeyondStorage.Reload;

[HarmonyPatch(typeof(ItemActionRanged))]
public class ItemActionRangedPatches {
    // Used For:
    //          Weapon Reload (check if allowed to reload)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionRanged.CanReload))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> ItemActionRanged_CanReload_Patch(IEnumerable<CodeInstruction> instructions) {
        // Skip if not enabled in config
        if (!BeyondStorage.Config.enableForReload) return instructions;
        var targetMethodString = $"{typeof(ItemActionRanged)}.{nameof(ItemActionRanged.CanReload)}";
        var codeInstructions = new List<CodeInstruction>(instructions);
        var lastBgt = codeInstructions.FindLastIndex(instruction => instruction.opcode == OpCodes.Bgt);
        LogUtil.Info($"Transpiling {targetMethodString}");
        if (lastBgt != -1) {
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Last BGT Index: {lastBgt}");
            // if (Ranged.CanReloadFromStorage(_itemValue) > 0)
            List<CodeInstruction> newCode = [
                // new CodeInstruction(OpCodes.Ldarg_0),
                // ldloc.2      // _itemValue
                new CodeInstruction(OpCodes.Ldloc_2),
                // Ranged.CanReloadFromStorage(ItemValue)
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Ranged), nameof(Ranged.CanReloadFromStorage))),
                // ldc.i4.0
                new CodeInstruction(OpCodes.Ldc_I4_0),
                // bgt
                codeInstructions[lastBgt].Clone()
            ];
            // Insert right below last BGT
            codeInstructions.InsertRange(lastBgt + 1, newCode);
            LogUtil.Info($"Successfully patched {targetMethodString}");
        } else {
            LogUtil.Error($"Failed to patch {targetMethodString}");
        }

        return codeInstructions.AsEnumerable();
    }
}