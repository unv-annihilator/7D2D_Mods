using System.Linq;
using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.ContainerLogic.Item;
using HarmonyLib;

namespace BeyondStorage.Item.Repair;

[HarmonyPatch(typeof(XUiC_ItemActionList))]
public class XUiCItemActionListPatches {
    // TODO: Possibly use this instead of IngredientList patch for item craft?
    // Used For:
    //      Item Repair (tracks item action list visibility)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_ItemActionList.Init))]
    private static void XUiC_ItemActionList_Init_Postfix(XUiC_ItemActionList __instance) {
        __instance.OnVisiblity += ActionList_VisibilityChanged;
    }

    // Capture when the visibility of the Action List is changed
    private static void ActionList_VisibilityChanged(XUiController _sender, bool _visible) {
        ItemRepair.ActionListVisible = _visible;
    }

    // Used For:
    //      Item Repair (captures if item actions list contains repairing)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_ItemActionList.SetCraftingActionList))]
    private static void XUiC_ItemActionList_SetCraftingActionList_Postfix(XUiC_ItemActionList __instance) {
        ActionList_UpdateVisibleActions(__instance);
    }

    // TODO: This one may not be needed
    // Used For:
    //      Item Repair (captures if item actions list contains repairing)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_ItemActionList.SetServiceActionList))]
    private static void XUiC_ItemActionList_SetServiceActionList_Postfix(XUiC_ItemActionList __instance) {
        ActionList_UpdateVisibleActions(__instance);
    }

    // Update `RepairActionShown` in `ContainerUtils` if list contains Repair action
    private static void ActionList_UpdateVisibleActions(XUiC_ItemActionList itemActionList) {
        if (!ModConfig.EnableForItemRepair()) return;
        ItemRepair.RepairActionShown = ActionList_HasRepair(itemActionList);
    }

    // Check if the list of actions contains Repair
    private static bool ActionList_HasRepair(XUiC_ItemActionList itemActionList) {
        return itemActionList.itemActionEntries.OfType<ItemActionEntryRepair>().Any();
    }
}