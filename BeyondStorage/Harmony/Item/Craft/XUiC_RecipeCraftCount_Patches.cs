using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Item.Craft;

[HarmonyPatch(typeof(XUiC_RecipeCraftCount))]
public class XUiCRecipeCraftCountPatches {
    // Used for:
    //          Item Crafting (gets max craftable amount)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiC_RecipeCraftCount.calcMaxCraftable))]
    // [HarmonyDebug]
    // TODO: could possibly be made cleaner?
    private static IEnumerable<CodeInstruction> XUiC_RecipeCraftCount_calcMaxCraftable_Patch(
        IEnumerable<CodeInstruction> instructions) {
        LogUtil.Info("Transpiling XUiC_RecipeCraftCount.calcMaxCraftable");
        // Append our itemStack array to current inventory
        var codes = new List<CodeInstruction>(instructions);
        var set = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetAllItemStacks)))
                continue;

            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Appending our item stacks to current inventory");

            codes.Insert(i + 2,
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetAllStorageStacksArrays))));
            set = true;
            break;
        }

        if (!set)
            LogUtil.Error("Failed to patch XUiC_RecipeCraftCount.calcMaxCraftable");
        else
            LogUtil.Info("Successfully patched XUiC_RecipeCraftCount.calcMaxCraftable");

        return codes.AsEnumerable();
    }
}