using System.Collections.Generic;
using BeyondStorage.Scripts;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeyondStorage._XUiM;

[HarmonyPatch(typeof(XUiM_PlayerInventory))]
public static class XUiM_PlayerInventory_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiM_PlayerInventory.GetAllItemStacks))]
    // [HarmonyDebug]
    [UsedImplicitly]
    public static void XUiM_PlayerInventory_GetAllItemStacks_Postfix(ref List<ItemStack> __result)
    {
        __result = ContainerUtils.GetAllStorageStacks(__result);
    }
}