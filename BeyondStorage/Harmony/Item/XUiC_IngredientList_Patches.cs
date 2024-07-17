using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Item;

[HarmonyPatch(typeof(XUiC_IngredientList))]
public class XUiCIngredientListPatches {
    // Used For:
    //      Item Crafting (toggles whether we're searching for items or not)
    //      Item Repairing (toggles whether we're searching for items or not)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_IngredientList.OnOpen))]
    private static void XUiC_IngredientList_OnOpen_Postfix() {
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("XUiC_IngredientList.OnOpen");
        ContainerUtils.IngredientListShown = true;
    }

    // Used For:
    //      Item Crafting (toggles whether we're searching for items or not)
    //      Item Repairing (toggles whether we're searching for items or not)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_IngredientList.OnClose))]
    private static void XUiC_IngredientList_OnClose_Postfix() {
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("XUiC_IngredientList.OnClose");
        ContainerUtils.IngredientListShown = false;
    }
}