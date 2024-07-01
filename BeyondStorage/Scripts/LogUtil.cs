namespace BeyondStorage.Scripts
{
    public static class LogUtil
    {
        public static void DebugLog(object str, bool prefix = true)
        {
            if (BeyondStorage.Config.isDebug)
                Log.Out((prefix ? "[" + BeyondStorage.ModInstance.DisplayName + "] " : "") + str);
        }
    }
}