using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.ContainerLogic.Vehicle;
using BeyondStorage.Scripts.Utils;
using HarmonyLib;

namespace BeyondStorage.Vehicle.Refuel;

[HarmonyPatch(typeof(EntityVehicle))]
public class EntityVehiclePatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EntityVehicle.hasGasCan))]
    private static void EntityVehicle_hasGasCan_Patch(EntityVehicle __instance, ref bool __result) {
        // Skip if not refueling from storage
        if (!ModConfig.EnableForVehicleRefuel()) return;
        // Update result of CanRefuel if nearby storage has required gas item
        __result = VehicleRefuel.CanRefuel(__instance, __result);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EntityVehicle.takeFuel))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> EntityVehicle_takeFuel_Transpiler(IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(EntityVehicle)}.{nameof(EntityVehicle.takeFuel)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand != AccessTools.Method(typeof(Bag), nameof(Bag.DecItem)))
                continue;
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Patching {targetMethodString}");
            found = true;
            List<CodeInstruction> newCode = [
                // ldloc.2      // _itemValue
                new CodeInstruction(OpCodes.Ldloc_2),
                // ldloc.3      // _count
                new CodeInstruction(OpCodes.Ldloc_3),
                // ldarg.2      // count
                new CodeInstruction(OpCodes.Ldarg_2),
                // VehicleRefuel.VehicleRefuelRemoveRemaining(_itemValue, _count, count)
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VehicleRefuel), nameof(VehicleRefuel.VehicleRefuelRemoveRemaining))),
                // stloc.3      // _count
                codes[i + 1].Clone()
            ];
            codes.InsertRange(i + 2, newCode);
            break;
        }

        if (!found)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}