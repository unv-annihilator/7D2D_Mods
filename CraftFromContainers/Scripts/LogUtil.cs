using UnityEngine;

namespace CraftFromContainers.Scripts
{
    public static class LogUtil
    {
        public static void DebugLog(object str, bool prefix = true)
        {
            if (CraftFromContainers.Config.isDebug)
                Log.Out((prefix ? "[" + CraftFromContainers.ModInstance.DisplayName + "] " : "") + str);
        }
    }
}