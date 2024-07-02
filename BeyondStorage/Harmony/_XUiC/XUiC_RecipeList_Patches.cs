﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeyondStorage.Scripts;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeyondStorage._XUiC;

[HarmonyPatch(typeof(XUiC_RecipeList))]
public class XUiC_RecipeList_Patches
{
    // [HarmonyDebug]
    // == BEFORE ==
    // IL_0081: callvirt     instance class ItemStack[] XUiC_ItemStackGrid::GetSlots()
    // IL_0086: callvirt     instance void class [mscorlib]System.Collections.Generic.List`1<class ItemStack>::AddRange(class [mscorlib]System.Collections.Generic.IEnumerable`1<!0/*class ItemStack*/>)
    // IL_008b: ldarg.0      // this [Label4]
    // IL_008c: ldloc.0      // updateStackList List<ItemStack> 
    // IL_008d: call         instance void XUiC_RecipeList::BuildRecipeInfosList(class [mscorlib]System.Collections.Generic.List`1<class ItemStack>)

    // == AFTER ==
    // IL_0084: callvirt ItemStack[] XUiC_ItemStackGrid::GetSlots()
    // IL_0089: callvirt System.Void System.Collections.Generic.List`1<ItemStack>::AddRange(System.Collections.Generic.IEnumerable`1<T>)
    // IL_008e: ldloc.0      // updateStackList List<ItemStack> [Label4]
    // IL_008f: call System.Void BeyondStorage.Scripts.ContainerUtils::AddAllStorageStacks(System.Collections.Generic.List`1<ItemStack>)
    // IL_0094: ldarg.0      // this 
    // IL_0095: ldloc.0      // updateStackList List<ItemStack> 
    // IL_0096: call System.Void XUiC_RecipeList::BuildRecipeInfosList(System.Collections.Generic.List`1<ItemStack>)
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(XUiC_RecipeList.Update))]
    // [HarmonyDebug]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> XUiC_RecipeList_Update_Patch(IEnumerable<CodeInstruction> instructions)
    {
        LogUtil.DebugLog("Transpiling XUiC_RecipeList.Update");
        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            // IL_008b: ldarg.0      // this
            // IL_008c: ldloc.0      // updateStackList
            // IL_008d: call         instance void XUiC_RecipeList::BuildRecipeInfosList(class [mscorlib]System.Collections.Generic.List`1<class ItemStack>)
            if (i <= 2 || codes[i].opcode != OpCodes.Call || (MethodInfo)codes[i].operand !=
                AccessTools.Method(typeof(XUiC_RecipeList), nameof(XUiC_RecipeList.BuildRecipeInfosList)))
                continue;
            LogUtil.DebugLog("XUiC_RecipeList.Update: Adding method to add items from all storages");
            // IL_008b: ldarg.0      // this [Label 4]
            var ci = codes[i - 2];
            // IL_008c: ldloc.0      // updateStackList
            var ciNew = new CodeInstruction(OpCodes.Ldloc_0);
            ci.MoveLabelsTo(ciNew);
            codes.Insert(i - 2,
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ContainerUtils),
                        nameof(ContainerUtils.AddAllStorageStacks))));
            codes.Insert(i - 2, ciNew);
            break;
        }

        return codes.AsEnumerable();
    }
}