using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.ItemActionPatches
{
    public static class EntryCraft
    {
        [HarmonyPatch(typeof(ItemActionEntryCraft), nameof(ItemActionEntryCraft.HasItems))]
        private static class ItemActionEntryCraft_OnActivated_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                LogUtil.DebugLog("Transpiling ItemActionEntryCraft.HasItems");
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetAllItemStacks)))
                    {
                        LogUtil.DebugLog("ItemActionEntryCraft.HasItems: Adding method to add items from all storages");
                        codes.Insert(i + 1,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.GetAllStorageStacks2))));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }
    }
}