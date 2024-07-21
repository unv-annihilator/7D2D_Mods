using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace SprintToggleFix._PlayerMoveController;

[HarmonyPatch(typeof(PlayerMoveController))]
public static class PlayerMoveControllerPatches {
    private const string LogPrefix = "[ToggleSprintFix]";

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerMoveController.Update))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> PlayerMoveController_Update_Transpiler(IEnumerable<CodeInstruction> instructions) {
        var targetStr = $"{typeof(PlayerMoveController)}.{nameof(PlayerMoveController.Update)}";
        // Prevents sprint getting stuck toggled on due to arbitrary delay check
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        Log.Out($"{LogPrefix} Transpiling {targetStr}");
        for (var i = 0; i < codes.Count; i++) {
            // ldfld        float32 PlayerMoveController::runInputTime
            if (codes[i].opcode != OpCodes.Ldfld || codes[i].operand as FieldInfo != AccessTools.Field(typeof(PlayerMoveController), nameof(PlayerMoveController.runInputTime))) continue;
            found = true;
            Log.Out($"{LogPrefix} Found {nameof(PlayerMoveController.runInputTime)}");
            // ldc.r4       0.2
            if (codes[i + 1].opcode != OpCodes.Ldc_R4) continue;
            var value = (float)codes[i + 1].operand;
            // Math.Abs float validation (avoids minor differences causing us to break)
            if (!(Math.Abs(value - 0.2f) < 0.000000002f)) continue;
            Log.Out($"{LogPrefix} Found 0.2 delay check");
            // Replacing:
            //      ldc.r4      0.2...
            // With:
            //      ldc.r4     -1.0
            codes[i + 1] = new CodeInstruction(OpCodes.Ldc_R4, (float)-1.0);
            Log.Out($"{LogPrefix} Updated to -1.0 check (always true)");
            break;
        }

        if (!found)
            Log.Error($"{LogPrefix} Failed to patch {targetStr}");
        else
            Log.Out($"{LogPrefix} Successfully patched {targetStr}");

        return codes.AsEnumerable();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerMoveController.stopMoving))]
#if DEBUG
    [HarmonyDebug]
#endif
    // ReSharper disable once InconsistentNaming
    private static void PlayerMoveController_stopMoving_Prefix(PlayerMoveController __instance) {
        // Prevents getting stuck on when expecting the player to stop movement
        var playerAction1 = __instance.playerInput.VehicleActions.Enabled ? __instance.playerInput.VehicleActions.Turbo : __instance.playerInput.Run;
#if DEBUG
        var targetStr = $"{typeof(PlayerMoveController)}.{nameof(PlayerMoveController.stopMoving)}";
        Log.Out(
            $"{LogPrefix} {targetStr} | runToggleActive: {__instance.runToggleActive}; entity Player running: {__instance.entityPlayerLocal.movementInput.running}; runPressedWhileActive {__instance.runPressedWhileActive}; sprintPressed: {playerAction1.IsPressed}");
#endif
        // Keep the state updated to what we have actually pressed
        __instance.entityPlayerLocal.movementInput.running = playerAction1.IsPressed;
        __instance.runPressedWhileActive = playerAction1.IsPressed;
    }
}