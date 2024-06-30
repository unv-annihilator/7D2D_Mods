using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.XUIM
{
    public static class PlayerInventory
    {
        [HarmonyPatch(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.HasItems))]
        private static class XUiM_PlayerInventory_HasItems_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                LogUtil.DebugLog("Transpiling XUiM_PlayerInventory.HasItems");
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                    if (i > 0 && i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_I4_0 &&
                        codes[i + 1].opcode == OpCodes.Ret)
                    {
                        LogUtil.DebugLog("XUiM_PlayerInventory.HasItems: Replacing return value with method");
                        codes.Insert(i, codes[i - 1].Clone());
                        codes.Insert(i, codes[i - 2].Clone());
                        codes.Insert(i,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils), nameof(ContainerUtils.GetTrueRemaining))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_1));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_0));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                        break;
                    }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory), nameof(XUiM_PlayerInventory.RemoveItems))]
        private static class XUiM_PlayerInventory_RemoveItems_Patch
        {
            public static void PostFix(IList<ItemStack> _itemStacks, int _multiplier)
            {
                foreach (var t in _itemStacks)
                {
                    var num = t.count * _multiplier;
                    LogUtil.DebugLog($"Need {num} {t.itemValue.ItemClass.GetItemName()}");
                }
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                LogUtil.DebugLog("Transpiling XUiM_PlayerInventory.RemoveItems");
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(Inventory), nameof(Inventory.DecItem)))
                    {
                        var ci = codes[i + 3];
                        var ciNew = new CodeInstruction(OpCodes.Ldarg_1);
                        ci.MoveLabelsTo(ciNew);
                        LogUtil.DebugLog("XUiM_PlayerInventory.RemoveItems: Adding method to remove from storages");
                        codes.Insert(i + 3,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.RemoveRemainingForCraft))));
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldloc_1));
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldloc_0));
                        codes.Insert(i + 3, ciNew);
                        break;
                    }

                return codes.AsEnumerable();
            }
        }
    }
}