using BeyondStorage.Scripts.Configuration;

namespace BeyondStorage.Scripts.Utils;

public static class LogUtil {
    private const string Prefix = "[BeyondStorage]";

    public static bool IsDebug() {
        return ModConfig.IsDebug();
    }

    public static void Info(string text) {
        Log.Out($"{Prefix}(Info) {text}");
    }

    public static void Error(string text) {
        Log.Error($"{Prefix}(Error) {text}");
    }

    public static void DebugLog(string text) {
        Log.Out($"{Prefix}(Debug) {text}");
    }

    public static void Warning(string text) {
        Log.Warning($"{Prefix}(Warn) {text}");
    }
}