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
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> EntityPlayerLocal_MoveByInput_Transpiler(IEnumerable<CodeInstruction> instructions) {
        const string prefix = "[AlphaJump]";
        var targetStr = $"{typeof(EntityPlayerLocal)}.{nameof(EntityPlayerLocal.MoveByInput)}";
        var codes = new List<CodeInstruction>(instructions);
        var lastBehaviour = -1;
        var endIndex = -1;
        Log.Out($"{prefix} Transpiling {targetStr}");
        for (var i = 0; i < codes.Count; i++) {
            // callvirt     instance void [UnityEngine.CoreModule]UnityEngine.Behaviour::set_enabled(bool)
            if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.PropertySetter(typeof(Behaviour), nameof(Behaviour.enabled))) {
                lastBehaviour = i + 2; // Skip current line and next
            }

            // stfld        bool EntityPlayerLocal::jumpTrigger
            if (lastBehaviour == -1 || codes[i].opcode != OpCodes.Stfld || (FieldInfo)codes[i].operand != AccessTools.Field(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.jumpTrigger))) continue;

            endIndex = i;
            Log.Out($"{prefix} Patching {targetStr}");

            // replacing:
            //      ldc.i4.1
            // with:
            //      ldarg.0
            codes[i - 1] = new CodeInstruction(OpCodes.Ldarg_0);

            // New code to insert
            List<CodeInstruction> newCode = [
                // ldfld        MovementInput EntityPlayerLocal::movementInput
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.movementInput))),
                // ldfld        bool MovementInput::jump
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MovementInput), nameof(MovementInput.jump)))
            ];
            // Insert code before: stfld        bool EntityPlayerLocal::jumpTrigger
            codes.InsertRange(i, newCode);
            Log.Out($"{prefix} {targetStr} - Patched {nameof(EntityPlayerLocal.jumpTrigger)} assignment");
            break;
        }

        if (endIndex > -1 && lastBehaviour > -1) {
            // Remove code starting at `lastBehaviour` for (endIndex - lastBehaviour - 1) count
            codes.RemoveRange(lastBehaviour, endIndex - lastBehaviour - 1);
            Log.Out($"{prefix} Successfully patched {targetStr}");
            /*
            BEFORE:
                if (this.movementInput.jump && (bool) (UnityEngine.Object) this.vp_FPController && !this.inputWasJump)
                  this.vp_FPController.enabled = true;
                if (!this.Jumping && !this.wasJumping && this.movementInput.jump && (this.onGround || this.isLadderAttached) && (UnityEngine.Object) this.AttachedToEntity == (UnityEngine.Object) null)
                  this.jumpTrigger = true;
                else if (this.wasLadderAttachedJump && !this.isLadderAttached && this.movementInput.jump && !this.inputWasJump)
                  this.canLadderAirAttach = true;

            AFTER:
                if (this.movementInput.jump && (bool) (UnityEngine.Object) this.vp_FPController && !this.inputWasJump)
                  this.vp_FPController.enabled = true;
                this.jumpTrigger = this.movementInput.jump;
                if (this.wasLadderAttachedJump && !this.isLadderAttached && this.movementInput.jump && !this.inputWasJump)
                  this.canLadderAirAttach = true;
             */
        } else {
            Log.Error($"{prefix} Failed to patch {targetStr}");
        }

        return codes.AsEnumerable();
    }
}