using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.ItemActionPatches
{
    public static class ActionRepair
    {
        [HarmonyPatch(typeof(ItemActionRepair), "CanRemoveRequiredResource")]
        private static class ItemActionRepair_CanRemoveRequiredResource_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (!CraftFromContainers.Config.enableForRepairAndUpgrade)
                    return codes;
                LogUtil.DebugLog("Transpiling ItemActionRepair.CanRemoveRequiredResource");
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(Bag), nameof(Bag.GetItemCount)))
                    {
                        LogUtil.DebugLog("ItemActionRepair.CanRemoveRequiredResource: Adding method to count items from all storages");
                        codes.Insert(i + 1,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.AddAllStoragesCountItem))));
                        codes.Insert(i + 1, new CodeInstruction(codes[i - 4].opcode, codes[i - 4].operand));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair), "RemoveRequiredResource")]
        private static class ItemActionRepair_RemoveRequiredResource_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (!CraftFromContainers.Config.enableForRepairAndUpgrade)
                    return codes;
                LogUtil.DebugLog("Transpiling ItemActionRepair.RemoveRequiredResource");
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(Bag), nameof(Bag.DecItem)))
                    {
                        LogUtil.DebugLog("ItemActionRepair.RemoveRequiredResource: Adding method to remove items from all storages");
                        codes.Insert(i + 1,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.RemoveRemainingForUpgrade))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_0));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair), "canRemoveRequiredItem")]
        private static class ItemActionRepair_canRemoveRequiredItem_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (!CraftFromContainers.Config.enableForRepairAndUpgrade)
                    return codes;
                LogUtil.DebugLog("Transpiling ItemActionRepair.canRemoveRequiredItem");
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(Bag), nameof(Bag.GetItemCount)))
                    {
                        LogUtil.DebugLog("ItemActionRepair.canRemoveRequiredItem: Adding method to count items from all storages");
                        codes.Insert(i + 1,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.AddAllStoragesCountItemStack))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(ItemActionRepair), "removeRequiredItem")]
        private static class ItemActionRepair_removeRequiredItem_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (!CraftFromContainers.Config.enableForRepairAndUpgrade)
                    return codes;
                LogUtil.DebugLog("Transpiling ItemActionRepair.removeRequiredItem");
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(Bag), nameof(Bag.DecItem)))
                    {
                        LogUtil.DebugLog("ItemActionRepair.removeRequiredItem: Adding method to remove items from all storages");
                        codes.Insert(i + 1,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.RemoveRemainingForRepair))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }
    }
}