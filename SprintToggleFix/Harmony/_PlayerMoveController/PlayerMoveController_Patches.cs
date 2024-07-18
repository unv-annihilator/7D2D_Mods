using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace SprintToggleFix._PlayerMoveController;

[HarmonyPatch(typeof(PlayerMoveController))]
public static class PlayerMoveControllerPatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerMoveController.Update))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> PlayerMoveController_Update_Transpiler(
        IEnumerable<CodeInstruction> instructions) {
        // Prevents sprint getting stuck toggled on due to arbitrary delay check
        var codes = new List<CodeInstruction>(instructions);
        Log.Out("[ToggleSprintFix] Transpiling PlayerMoveController.Update");
        for (var i = 0; i < codes.Count; i++) {
            // ldfld        float32 PlayerMoveController::runInputTime
            if (codes[i].opcode != OpCodes.Ldfld || codes[i].operand as FieldInfo != AccessTools.Field(typeof(PlayerMoveController), nameof(PlayerMoveController.runInputTime))) continue;
            Log.Out("[ToggleSprintFix] Found runInputTime");
            // ldc.r4       0.2
            if (codes[i + 1].opcode != OpCodes.Ldc_R4) continue;
            Log.Out("[ToggleSprintFix] Found 0.2 delay check");
            var value = (float)codes[i + 1].operand;
            if (!(Math.Abs(value - 0.2f) < 0.000000002f)) continue;
            // ldc.r4 -1
            codes[i + 1] = new CodeInstruction(OpCodes.Ldc_R4, (float)-1.0);
            Log.Out("[ToggleSprintFix] Updated to -1.0 check (always true)");
            break;
        }

        return codes.AsEnumerable();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerMoveController.stopMoving))]
    // [HarmonyDebug]
    private static void PlayerMoveController_stopMoving_Postfix(PlayerMoveController __instance) {
        // Prevents getting stuck on when expecting the player to stop movement
        // Log.Out($"PlayerMoveController.stopMoving | runToggleActive: {__instance.runToggleActive}; entity Player running: {__instance.entityPlayerLocal.movementInput.running}; runPressedWhileActive {__instance.runPressedWhileActive}");
        __instance.runToggleActive = false;
        __instance.entityPlayerLocal.movementInput.running = false;
        __instance.runPressedWhileActive = false;
    }
}