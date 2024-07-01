using BeyondStorage.Scripts;
using HarmonyLib;

namespace BeyondStorage.GameManagerPatches
{
    public static class StartGameHook
    {
        [HarmonyPatch(typeof(GameManager), "StartGame")]
        private static class GameManager_StartGame_Patch
        {
            private static void PostFix()
            {
                ContainerUtils.Init();
            }
        }
    }
}