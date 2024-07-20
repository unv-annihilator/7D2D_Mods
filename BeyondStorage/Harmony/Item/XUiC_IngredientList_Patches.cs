using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Item;

[HarmonyPatch(typeof(XUiC_IngredientList))]
public class XUiCIngredientListPatches {
    private readonly static string OnOpenTargetString = $"{typeof(XUiC_IngredientList)}.{nameof(XUiC_IngredientList.OnOpen)}";

    private readonly static string OnCloseTargetString = $"{typeof(XUiC_IngredientList)}.{nameof(XUiC_IngredientList.OnClose)}";

    // Used For:
    //      Item Crafting (toggles whether we're searching for items or not)
    //      Item Repairing (toggles whether we're searching for items or not)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_IngredientList.OnOpen))]
    private static void XUiC_IngredientList_OnOpen_Postfix() {
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"{OnOpenTargetString}");
        ContainerUtils.IngredientListShown = true;
    }

    // Used For:
    //      Item Crafting (toggles whether we're searching for items or not)
    //      Item Repairing (toggles whether we're searching for items or not)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_IngredientList.OnClose))]
    private static void XUiC_IngredientList_OnClose_Postfix() {
        if (LogUtil.IsDebugEnabled()) LogUtil.DebugLog($"{OnCloseTargetString}");
        ContainerUtils.IngredientListShown = false;
    }
}