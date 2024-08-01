using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts.ContainerLogic.Item;
using BeyondStorage.Scripts.Utils;
using HarmonyLib;

namespace BeyondStorage.Item.Craft;

[HarmonyPatch(typeof(XUiC_RecipeCraftCount))]
public class XUiCRecipeCraftCountPatches {
    // Used for:
    //          Item Crafting (gets max craftable amount)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiC_RecipeCraftCount.calcMaxCraftable))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> XUiC_RecipeCraftCount_calcMaxCraftable_Patch(
        IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(XUiC_RecipeCraftCount)}.{nameof(XUiC_RecipeCraftCount.calcMaxCraftable)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        // Append our itemStack array to current inventory
        var codes = new List<CodeInstruction>(instructions);
        var set = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetAllItemStacks)))
                continue;

            if (LogUtil.IsDebug()) LogUtil.DebugLog("Appending our item stacks to current inventory");

            // ItemCraft.MaxCraftGetAllStorageStacks(this.xui.PlayerInventory.GetAllItemStacks()).ToArray()
            codes.Insert(i + 1,
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ItemCraft), nameof(ItemCraft.ItemCraftMaxGetAllStorageStacks))));
            set = true;
            break;
        }

        if (!set)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}