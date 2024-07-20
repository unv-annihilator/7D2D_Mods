using System.Linq;
using BeyondStorage.Scripts;
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
        __instance.OnVisiblity += VisibilityChanged;
    }

    // Capture when the visibility of the Action List is changed
    private static void VisibilityChanged(XUiController _sender, bool _visible) {
        if (!BeyondStorage.Config.enableForItemRepair)
            return;
        // if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"{_sender.GetType()} {_visible}");
        ContainerUtils.ActionListVisible = _visible;
    }

    // Used For:
    //      Item Repair (captures if item actions list contains repairing)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_ItemActionList.SetCraftingActionList))]
    private static void XUiC_ItemActionList_SetCraftingActionList_Postfix(XUiC_ItemActionList __instance) {
        if (BeyondStorage.Config.enableForItemRepair)
            UpdateRepair(__instance);
    }

    // TODO: This one may not be needed
    // Used For:
    //      Item Repair (captures if item actions list contains repairing)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_ItemActionList.SetServiceActionList))]
    private static void XUiC_ItemActionList_SetServiceActionList_Postfix(XUiC_ItemActionList __instance) {
        if (BeyondStorage.Config.enableForItemRepair)
            UpdateRepair(__instance);
    }

    // Update `RepairActionShown` in `ContainerUtils` if list contains Repair action
    private static void UpdateRepair(XUiC_ItemActionList itemActionList) {
        var repairFound = HasRepair(itemActionList);
        // if (LogUtil.IsDebugEnabled())
        //     LogUtil.DebugLog($"Repair Entry Found {repairFound}");
        ContainerUtils.RepairActionShown = repairFound;
    }

    // Check if the list of actions contains Repair
    private static bool HasRepair(XUiC_ItemActionList itemActionList) {
        return itemActionList.itemActionEntries.OfType<ItemActionEntryRepair>().Any();
    }
}