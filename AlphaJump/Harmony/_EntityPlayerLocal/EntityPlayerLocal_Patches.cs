using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace AlphaJump._EntityPlayerLocal;

[HarmonyPatch(typeof(EntityPlayerLocal))]
public class EntityPlayerLocalPatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EntityPlayerLocal.MoveByInput))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> EntityPlayerLocal_MoveByInput_Transpiler(
        IEnumerable<CodeInstruction> instructions) {
        var codes = new List<CodeInstruction>(instructions);
        var lastBehaviour = -1;
        var endIndex = -1;
        Log.Out("[AlphaJump]: Transpiling EntityPlayerLocal.MoveByInput");
        for (var i = 0; i < codes.Count; i++)
            // IL_02cb: callvirt     instance void [UnityEngine.CoreModule]UnityEngine.Behaviour::set_enabled(bool)
            if (codes[i].opcode == OpCodes.Callvirt &&
                (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Behaviour), "set_enabled")) {
                lastBehaviour = i + 2; // Skip current line and next
            } else if (codes[i].opcode == OpCodes.Stfld && ((FieldInfo)codes[i].operand).Name == "jumpTrigger") {
                // stfld        bool EntityPlayerLocal::jumpTrigger
                endIndex = i;
                Log.Out("[AlphaJump]: Updating EntityPlayerLocal.MoveByInput");
                // replacing: ldc.i4.1
                codes[i - 1] = new CodeInstruction(OpCodes.Ldarg_0);
                codes[i] = new CodeInstruction(OpCodes.Stfld,
                    AccessTools.Field(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.jumpTrigger)));
                codes.Insert(i,
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(MovementInput), nameof(MovementInput.jump))));
                codes.Insert(i,
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.movementInput))));
                Log.Out("[AlphaJump]: EntityPlayerLocal.MoveByInput - Updated jumpTrigger assignment");
                break;
            }

        if (endIndex > -1 && lastBehaviour > -1)
            codes.RemoveRange(lastBehaviour, endIndex - lastBehaviour - 1);
        else
            Log.Error("[AlphaJump]: Failed to patch EntityPlayerLocal.OnUpdateLive");
        return codes.AsEnumerable();
    }
}