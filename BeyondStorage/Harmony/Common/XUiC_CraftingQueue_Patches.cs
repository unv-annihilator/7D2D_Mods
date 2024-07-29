using BeyondStorage.Scripts.Common;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace BeyondStorage.Common;

[HarmonyPatch(typeof(XUiC_CraftingQueue))]
public class XUiCCraftingQueuePatches {
    // TODO: Remove this if it's fixed or determine steps to reproduce and send bug report to TFP
    // possible index out of bounds seems to get hit when opening workbenches?
    // seems like vanilla bug but avoiding it here as I believe it fails more gracefully in vanilla
    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiC_CraftingQueue.AddRecipeToCraftAtIndex))]
    private static bool XUiC_CraftingQueue_AddRecipeToCraftAtIndex_Prefix(XUiC_CraftingQueue __instance, ref bool __result, int _index) {
        var inBounds = _index < __instance.queueItems.Length;
        if (inBounds) return true;
        LogUtil.Error("XUiC_CraftingQueue.AddRecipeToCraftAtIndex OutOfBounds!");
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Queue Length: {__instance.queueItems.Length}; _index: {_index}; {_index >= __instance.queueItems.Length}");
        __result = false;
        return false;
    }
}