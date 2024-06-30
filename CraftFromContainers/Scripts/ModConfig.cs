using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;

namespace CraftFromContainers.Scripts
{
    public class ModConfig
    {
        public bool enableForRepairAndUpgrade = true;
        public bool isDebug = true;
        public bool modEnabled = true;
        public float range = -1;

        private static string GetAssetPath(object obj, bool create = false)
        {
            return GetAssetPath(obj.GetType().Namespace, create);
        }

        private static string GetAssetPath(string name, bool create = false)
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                throw new InvalidOperationException(), name);
            if (create && !Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static ModConfig LoadConfig(CraftFromContainers context)
        {
            var path = Path.Combine(GetAssetPath(context, true), "config.json");
            var config = !File.Exists(path)
                ? new ModConfig()
                : JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(path));
            File.WriteAllText(path,
                JsonConvert.SerializeObject(config, (Newtonsoft.Json.Formatting)Formatting.Indented));
            return config;
        }
    }
}