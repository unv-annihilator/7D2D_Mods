using System.IO;
using BeyondStorage.Scripts.Common;
using Newtonsoft.Json;
using static System.IO.File;

namespace BeyondStorage.Scripts.Configuration;

internal static class ModConfig {
    private const string ConfigFileName = "config.json";

    public static Config LoadConfig(BeyondStorage context) {
        var path = Path.Combine(FileUtil.GetAssetPath(context, true), ConfigFileName);
        var config = !Exists(path)
            ? new Config()
            : JsonConvert.DeserializeObject<Config>(ReadAllText(path));
        WriteAllText(path,
            JsonConvert.SerializeObject(config, Formatting.Indented));
        return config;
    }

    internal class Config {
        // if set true nearby containers will be used for block repairs
        public bool enableForBlockRepair = true;

        // if set true nearby containers will be used for block upgrades
        public bool enableForBlockUpgrade = true;

        // if set true nearby containers will be used for item repairs
        // disable if you experience lag
        public bool enableForItemRepair = true;

        // if set true nearby containers will be used for gun reloading
        public bool enableForReload = true;

        // if set true additional logging will be printed to logs/console
        public bool isDebug = false;

        // if set to true it will ignore tile entities that aren't Storage Containers (crates)
        // otherwise will check all lootable containers placed by player(s)
        public bool onlyStorageCrates = false;

        // if set to true it will try and pull items from nearby vehicle storages
        public bool pullFromVehicleStorage = false;

        // How far to pull from (-1 is infinite range, only limited by chunks loaded)
        public float range = -1;
    }
}