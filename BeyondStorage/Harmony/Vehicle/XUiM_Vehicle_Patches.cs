using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.ContainerLogic.Vehicle;
using HarmonyLib;

namespace BeyondStorage.Vehicle;

[HarmonyPatch(typeof(XUiM_Vehicle))]
public class XUiMVehiclePatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiM_Vehicle.RepairVehicle))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> XUiM_Vehicle_RepairVehicle_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        if (!BeyondStorage.Config.enableForVehicleRepair) return instructions;
        var targetMethodString = $"{typeof(XUiM_Vehicle)}.{nameof(XUiM_Vehicle.RepairVehicle)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            // loop until we hit missing item error sound
            if (codes[i].opcode != OpCodes.Ldstr || codes[i].operand as string != "misc/missingitemtorepair")
                continue;
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Patching {targetMethodString}");
            found = true;
            // define new lable
            var newLabel = generator.DefineLabel();
            // add new label to ldstr "misc/missingitemtorepair"
            codes[i].labels.Add(newLabel);
            // use new label in our jump
            var ci = new CodeInstruction(OpCodes.Ble_S, newLabel);
            List<CodeInstruction> newCode = [
                //  ldarg.0      // _xui
                new CodeInstruction(OpCodes.Ldarg_0),
                //  ldarg.1      // vehicle
                new CodeInstruction(OpCodes.Ldarg_1),
                //  ldloc.0      // _itemValue
                new CodeInstruction(OpCodes.Ldloc_0),
                // VehicleRepair.VehicleRepairRemoveRemaining(XUI _xui, Vehicle vehicle, ItemValue _itemValue)
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VehicleRepair), nameof(VehicleRepair.VehicleRepairRemoveRemaining))),
                // 0
                new CodeInstruction(OpCodes.Ldc_I4_0),
                // if (VehicleRepair.VehicleRepairRemoveRemaining(...) > 0)
                ci,
                // return true
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret)
            ];

            codes.InsertRange(i, newCode);
            break;
        }

        if (!found)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}