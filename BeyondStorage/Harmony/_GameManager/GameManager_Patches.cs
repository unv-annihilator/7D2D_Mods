using BeyondStorage.Scripts;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeyondStorage._GameManager;

[HarmonyPatch(typeof(GameManager))]
public static class GameManager_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.StartGame))]
    // [HarmonyDebug]
    [UsedImplicitly]
    private static void GameManager_StartGame_PostFix()
    {
        ContainerUtils.Init();
    }
}