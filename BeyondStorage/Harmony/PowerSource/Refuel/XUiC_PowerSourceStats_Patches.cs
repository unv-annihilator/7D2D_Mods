using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts.ContainerLogic.PowerSource;
using BeyondStorage.Scripts.Utils;
using HarmonyLib;
using UniLinq;

namespace BeyondStorage.PowerSource.Refuel;

[HarmonyPatch(typeof(XUiC_PowerSourceStats))]
public class XUiCPowerSourceStatsPatches {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiC_PowerSourceStats.BtnRefuel_OnPress))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> XUiC_PowerSourceStats_BtnRefuel_OnPress_Transpiler(IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(XUiC_PowerSourceStats)}.{nameof(XUiC_PowerSourceStats.BtnRefuel_OnPress)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand != AccessTools.Method(typeof(Bag), nameof(Bag.DecItem)))
                continue;
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"Patching {targetMethodString}");
            found = true;
            List<CodeInstruction> newCode = [
                // ldloc.s      _itemValue
                codes[i - 5].Clone(),
                // ldloc.s      _count2 (last removed count)
                codes[i + 2].Clone(),
                // ldloc.2      // _count1
                new CodeInstruction(OpCodes.Ldloc_2),
                // conv.i4      (int) _count1
                new CodeInstruction(OpCodes.Conv_I4),
                // PowerSourceRefuel.RefuelRemoveRemaining(ItemValue itemValue, int lastRemoved, int totalNeeded)
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerSourceRefuel), nameof(PowerSourceRefuel.RefuelRemoveRemaining))),
                // stloc.s      _count2     |   update result
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