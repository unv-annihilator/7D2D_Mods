using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.XUIC
{
    public static class IngredientEntry
    {
        [HarmonyPatch(typeof(XUiC_IngredientEntry), nameof(XUiC_IngredientEntry.GetBindingValue))]
        private static class XUiC_IngredientEntry_GetBindingValue_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                LogUtil.DebugLog("Transpiling XUiC_IngredientEntry.GetBindingValue");
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetItemCount),
                            new[] { typeof(ItemValue) }))
                    {
                        LogUtil.DebugLog("XUiC_IngredientEntry.GetBindingValue: Adding method to add item counts from all storages");
                        codes.Insert(i + 1,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.AddAllStoragesCountEntry))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }

                return codes.AsEnumerable();
            }
        }
    }
}