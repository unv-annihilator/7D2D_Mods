using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.GameManagerPatches
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