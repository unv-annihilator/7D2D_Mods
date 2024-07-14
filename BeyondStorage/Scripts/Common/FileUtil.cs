using System;
using System.IO;
using System.Reflection;

namespace BeyondStorage.Scripts.Common;

internal static class FileUtil {
    internal static string GetAssetPath(object obj, bool create = false) {
        return GetAssetPath(obj.GetType().Namespace, create);
    }

    private static string GetAssetPath(string name, bool create = false) {
        var path = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(),
            name);
        if (create && !Directory.Exists(path)) Directory.CreateDirectory(path);

        return path;
    }
}