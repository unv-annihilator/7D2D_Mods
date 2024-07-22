using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.ContainerLogic;
using HarmonyLib;

namespace BeyondStorage.Common;

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.StartGame))]
    private static void GameManager_StartGame_Postfix() {
        if (LogUtil.IsDebug()) LogUtil.DebugLog("StartGame: Initializing...");
        ContainerUtils.Init();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.Disconnect))]
    private static void GameManager_Disconnect_Postfix() {
        if (LogUtil.IsDebug()) LogUtil.DebugLog("Disconnect: Cleaning up...");
        ContainerUtils.Cleanup();
    }
}