using BeyondStorage.Scripts;
using BeyondStorage.Scripts.Common;
using HarmonyLib;

namespace BeyondStorage.Common;

[HarmonyPatch(typeof(GameManager))]
public static class GameManagerPatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.StartGame))]
    // [HarmonyDebug]
    private static void GameManager_StartGame_PostFix() {
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("StartGame: Initializing ContainerUtils");

        ContainerUtils.Init();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.Disconnect))]
    // [HarmonyDebug]
    private static void GameManager_Disconnect_PostFix() {
        if (BeyondStorage.Config.isDebug) LogUtil.DebugLog("Disconnect: Cleaning up ContainerUtils");

        ContainerUtils.Cleanup();
    }
}