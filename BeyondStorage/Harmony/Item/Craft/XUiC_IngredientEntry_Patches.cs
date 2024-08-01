using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts.ContainerLogic.Item;
using BeyondStorage.Scripts.Utils;
using HarmonyLib;

namespace BeyondStorage.Item.Craft;

[HarmonyPatch(typeof(XUiC_IngredientEntry))]
public class XUiCIngredientEntryPatches {
    // Used for:
    //      Item Crafting (shows item count available in crafting window(s))
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiC_IngredientEntry.GetBindingValue))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static IEnumerable<CodeInstruction> XUiC_IngredientEntry_GetBindingValue_Patch(
        IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(XUiC_IngredientEntry)}.{nameof(XUiC_IngredientEntry.GetBindingValue)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        var found = false;
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand != AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetItemCount), [typeof(ItemValue)]))
                continue;

            if (LogUtil.IsDebug()) LogUtil.DebugLog("Adding method to add item counts from all storages");

            found = true;
            List<CodeInstruction> newCode = [
                // Ldarg_0      this
                new CodeInstruction(OpCodes.Ldarg_0),
                // ItemCommon.EntryBindingAddAllStorageCount(this.xui.PlayerInventory.GetItemCount(this.ingredient.itemValue)), this)
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ItemCraft), nameof(ItemCraft.EntryBindingAddAllStorageCount)))
            ];
            codes.InsertRange(i + 1, newCode);
        }

        if (!found)
            LogUtil.Error($"Failed to patch {targetMethodString}");
        else
            LogUtil.Info($"Successfully patched {targetMethodString}");

        return codes.AsEnumerable();
    }
}