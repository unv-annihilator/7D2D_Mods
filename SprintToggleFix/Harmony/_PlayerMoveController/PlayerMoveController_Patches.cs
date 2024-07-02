using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace SprintToggleFix._PlayerMoveController
{
    [HarmonyPatch(typeof(PlayerMoveController))]
    public static class PlayerMoveController_Patches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerMoveController.Update))]
        // [HarmonyDebug]
        private static IEnumerable<CodeInstruction> PlayerMoveController_Update_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            Log.Out("[ToggleSprintFix] Transpiling PlayerMoveController.Update");
            for (var i = 0; i < codes.Count; i++)
                if (codes[i].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i].operand).Name == "runInputTime")
                {
                    Log.Out("[ToggleSprintFix] Found runInputTime");
                    if (codes[i + 1].opcode == OpCodes.Ldc_R4)
                    {
                        Log.Out("[ToggleSprintFix] Found 0.2 delay");
                        var value = (float)codes[i + 1].operand;
                        if (Math.Abs(value - 0.2f) < 0.000000002f)
                        {
                            codes[i + 1] = new CodeInstruction(OpCodes.Ldc_R4, (float)0.0);
                            Log.Out("[ToggleSprintFix] Updated to 0.0 delay");
                            break;
                        }
                    }
                }

            return codes.AsEnumerable();
        }
    }
}