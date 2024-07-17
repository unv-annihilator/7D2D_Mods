using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Item.Repair;

[HarmonyPatch(typeof(ItemActionEntryRepair))]
public class ItemActionEntryRepairPatches {
    // Used For:
    //      Item Repair (Allows Repair)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionEntryRepair.OnActivated))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> ItemActionEntryRepair_OnActivated_Patch(
        IEnumerable<CodeInstruction> instructions) {
        if (!BeyondStorage.Config.enableForItemRepair) return instructions;

        LogUtil.Info("Transpiling ItemActionEntryRepair.OnActivated");
        var codes = new List<CodeInstruction>(instructions);
        var startIndex = -1;
        var endIndex = -1;
        for (var i = 0; i < codes.Count; i++) {
            if (startIndex != -1 && codes[i].opcode == OpCodes.Ble) {
                if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Patching ItemActionEntryRepair.OnActivated");

                endIndex = i;
                List<CodeInstruction> newCode = [
                    codes[startIndex - 4].Clone(),
                    // callvirt     instance int32 XMLData.Item.ItemData::get_Id()
                    codes[startIndex - 3].Clone(),
                    // ldc.i4.0
                    codes[startIndex - 2].Clone(),
                    // newobj       instance void ItemValue::.ctor(int32, bool)
                    codes[startIndex - 1].Clone(),
                    // ldloc.s      _count
                    codes[startIndex + 4].Clone(),
                    // ContainerUtils.GetTrueItemRepairCount(new ItemValue(itemClass.Id))
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetTrueItemRepairCount))),
                    // ldloc.s      int32
                    codes[startIndex + 1].Clone(),
                    // call         int32 [UnityEngine.CoreModule]UnityEngine.Mathf::Min(int32, int32)
                    codes[startIndex + 2].Clone(),
                    // stloc.s      _count
                    codes[startIndex + 3].Clone()
                ];
                // Insert below start
                codes.InsertRange(startIndex + 4, newCode);
                break;
            }

            if (startIndex != -1 || codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(
                    typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetItemCount), [
                        typeof(ItemValue)
                    ]))
                continue;

            startIndex = i;
            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Found start");
        }

        if (startIndex == -1 || endIndex == -1)
            LogUtil.Error("Failed to patch ItemActionEntryRepair.OnActivated");
        else
            LogUtil.Info("Successfully patched ItemActionEntryRepair.OnActivated");

        return codes.AsEnumerable();
    }

    // Used For:
    //      Item Repair (Button Enabled)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ItemActionEntryRepair.RefreshEnabled))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> ItemActionEntryRepair_RefreshEnabled_Patch(
        IEnumerable<CodeInstruction> instructions) {
        if (!BeyondStorage.Config.enableForItemRepair) return instructions;

        LogUtil.Info("Transpiling ItemActionEntryRepair.RefreshEnabled");
        var startIndex = -1;
        var endIndex = -1;
        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++) {
            if (startIndex != -1 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Bgt) {
                endIndex = i;
                if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Found end");

                List<CodeInstruction> newCode = [
                    codes[startIndex - 4].Clone(),
                    // getId
                    codes[startIndex - 3].Clone(),
                    // Ldc_I4_0
                    codes[startIndex - 2].Clone(),
                    // new ItemValue(itemClass.Id, 0)
                    codes[startIndex - 1].Clone(),
                    // ContainerUtils.GetItemCount(new ItemValue(itemClass.Id, 0))
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetItemCountForItem))),
                    // ldloc.s  'int32'
                    codes[startIndex + 1].Clone(),
                    // call         int32 [UnityEngine.CoreModule]UnityEngine.Mathf::Min(int32, int32)
                    codes[startIndex + 2].Clone(),
                    // ldloc.3      // itemClass
                    codes[startIndex + 3].Clone(),
                    // ldfld        class DataItem`1<int32> ItemClass::RepairAmount
                    codes[startIndex + 4].Clone(),
                    // callvirt     instance !0/*int32*/ class DataItem`1<int32>::get_Value()
                    codes[startIndex + 5].Clone(),
                    // mul
                    codes[startIndex + 6].Clone(),
                    // ldc.i4.0
                    codes[startIndex + 7].Clone(),
                    // bgt.s        IL_013c
                    codes[startIndex + 8].Clone()
                ];
                // Insert our code below the previous jump (Bgt)
                codes.InsertRange(endIndex + 2, newCode);
                // Small smoke test that we're copying the code we expect
                if (startIndex + 8 != endIndex + 1)
                    LogUtil.Error($"Expected Equals False | Start+8 {startIndex + 8} == End+1 {endIndex + 1}");

                break;
            }

            if (startIndex != -1 || codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetItemCount), [
                    typeof(ItemValue)
                ]))
                continue;

            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Found start");

            startIndex = i;
        }

        if (startIndex == -1 || endIndex == -1)
            LogUtil.Error("Failed to patch ItemActionEntryRepair.RefreshEnabled");
        else
            LogUtil.Info("Successfully patched ItemActionEntryRepair.RefreshEnabled");

        return codes.AsEnumerable();
    }
}