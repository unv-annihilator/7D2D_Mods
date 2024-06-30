// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using System.Reflection.Emit;
// using CraftFromContainers.Scripts;
// using HarmonyLib;
//
// namespace CraftFromContainers.ItemActionPatches
// {
//     public class EntryRepair
//     {
//         [HarmonyPatch(typeof(ItemActionEntryRepair), nameof(ItemActionEntryRepair.OnActivated))]
//         private static class ItemActionEntryRepair_OnActivated_Patch
//         {
//             public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//             {
//                 LogUtil.DebugLog("Transpiling ItemActionEntryRepair.OnActivated");
//                 var codes = new List<CodeInstruction>(instructions);
//                 for (var i = 0; i < codes.Count; i++)
//                     if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
//                         AccessTools.Method(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.GetItemCount)))
//                     {
//                         LogUtil.DebugLog("ItemActionEntryRepair.OnActivated: Adding method to add items from all storages");
//                         codes.Insert(i + 1,
//                             new CodeInstruction(OpCodes.Call,
//                                 AccessTools.Method(typeof(CraftFromContainers),
//                                     nameof(ContainerUtils.GetAllStorageStacks2))));
//                         break;
//                     }
//         
//                 return codes.AsEnumerable();
//             }
//         }
//     }
// }