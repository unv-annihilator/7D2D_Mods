using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Item;

[HarmonyPatch(typeof(XUiC_IngredientEntry))]
public class XUiCIngredientEntryPatches {
    // Used for:
    //      Item Crafting (shows item count available in crafting window(s))
    //      Item Repair (removes items with Bag DecItem)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiC_IngredientEntry.GetBindingValue))]
    // [HarmonyDebug]
    private static IEnumerable<CodeInstruction> XUiC_IngredientEntry_GetBindingValue_Patch(
        IEnumerable<CodeInstruction> instructions) {
        var targetMethodString = $"{typeof(XUiC_IngredientEntry)}.{nameof(XUiC_IngredientEntry.GetBindingValue)}";
        LogUtil.Info($"Transpiling {targetMethodString}");
        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++) {
            if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetItemCount),
                    new[] { typeof(ItemValue) }))
                continue;

            if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Adding method to add item counts from all storages");

            // TODO: possibly clean this up
            codes.Insert(i + 1,
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.AddAllStoragesCountEntry))));
            codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
        }

        return codes.AsEnumerable();
    }
}