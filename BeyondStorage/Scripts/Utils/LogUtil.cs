using System;

namespace BeyondStorage.Scripts.Common;

public static class LogUtil {
    private const string Prefix = "[BeyondStorage]";

    private static void Out(LogLevel level, string text) {
        switch (level) {
            case LogLevel.Debug:
                Log.Out($"{Prefix}(Debug) {text}");
                break;
            case LogLevel.Info:
                Log.Out($"{Prefix}(Info) {text}");
                break;
            case LogLevel.Error:
                Log.Error($"{Prefix}(Error) {text}");
                break;
            // case LogLevel.Verbose:
            //     Log.Out($"{Prefix}(Verbose) {text}");
            //     break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    public static bool IsDebug() {
        return BeyondStorage.Config.isDebug;
    }

    // public static void Verbose(string text) {
    //     Out(LogLevel.Verbose, text);
    // }

    public static void Info(string text) {
        Out(LogLevel.Info, text);
    }

    public static void Error(string text) {
        Out(LogLevel.Error, text);
    }

    public static void DebugLog(string text) {
        Out(LogLevel.Debug, text);
    }

    private enum LogLevel {
        Debug,
        Info,

        // Verbose,
        Error
    }
}