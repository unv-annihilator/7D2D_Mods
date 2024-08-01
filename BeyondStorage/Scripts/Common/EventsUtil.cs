using BeyondStorage.Scripts.ContainerLogic;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.Common;

public static class EventsUtil {
    public static void GameStartDone() {
        if (LogUtil.IsDebug()) LogUtil.DebugLog("Game Start: Initializing...");
        ContainerUtils.Init();
    }

    public static void GameShutdown() {
        if (LogUtil.IsDebug()) LogUtil.DebugLog("Game Shutdown: Cleaning up...");
        ContainerUtils.Cleanup();
    }
}