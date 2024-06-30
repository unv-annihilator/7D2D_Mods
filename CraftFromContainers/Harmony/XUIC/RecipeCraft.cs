using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.XUIC
{
    public static class RecipeCraft
    {
        [HarmonyPatch(typeof(XUiC_RecipeCraftCount), "calcMaxCraftable")]
        private static class XUiC_RecipeCraftCount_calcMaxCraftable_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                LogUtil.DebugLog("Transpiling XUiC_RecipeCraftCount.calcMaxCraftable");
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetAllItemStacks)))
                    {
                        LogUtil.DebugLog("XUiC_RecipeCraftCount.calcMaxCraftable: Adding method to add items from all storages");
                        codes.Insert(i + 2,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.GetAllStorageStacks))));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }
    }
}