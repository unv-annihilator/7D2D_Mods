using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.Server;

// ReSharper disable once ClassNeverInstantiated.Global
public class NetPackageBeyondStorageConfig : NetPackage {
    private const ushort ConfigVersion = 1;
    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public override void write(PooledBinaryWriter binaryWriter) {
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Sending config, version {ConfigVersion}, from server to client.");
        base.write(binaryWriter);
        binaryWriter.Write(ConfigVersion);
        binaryWriter.Write(ModConfig.ClientConfig.range);
        binaryWriter.Write(ModConfig.ClientConfig.enableForBlockRepair);
        binaryWriter.Write(ModConfig.ClientConfig.enableForBlockUpgrade);
        binaryWriter.Write(ModConfig.ClientConfig.enableForGeneratorRefuel);
        binaryWriter.Write(ModConfig.ClientConfig.enableForItemRepair);
        binaryWriter.Write(ModConfig.ClientConfig.enableForReload);
        binaryWriter.Write(ModConfig.ClientConfig.enableForVehicleRefuel);
        binaryWriter.Write(ModConfig.ClientConfig.enableForVehicleRepair);
        binaryWriter.Write(ModConfig.ClientConfig.onlyStorageCrates);
        binaryWriter.Write(ModConfig.ClientConfig.pullFromVehicleStorage);
        // binaryWriter.Write(BeyondStorage.ClientConfig.pullFromDroneStorage);
    }

    public override void read(PooledBinaryReader reader) {
        var configVersion = reader.ReadUInt16();
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Received config, version {configVersion}, from server.");
        if (configVersion != ConfigVersion) {
            // TODO: send mod version from server?
            LogUtil.Error("Invalid configuration version sent from server!!! Please update your mod to the same version as the server.");
            return;
        }

        // update server config (or set if it's first time)
        ModConfig.ServerConfig.range = reader.ReadSingle();
        ModConfig.ServerConfig.enableForBlockRepair = reader.ReadBoolean();
        ModConfig.ServerConfig.enableForBlockUpgrade = reader.ReadBoolean();
        ModConfig.ServerConfig.enableForGeneratorRefuel = reader.ReadBoolean();
        ModConfig.ServerConfig.enableForItemRepair = reader.ReadBoolean();
        ModConfig.ServerConfig.enableForReload = reader.ReadBoolean();
        ModConfig.ServerConfig.enableForVehicleRefuel = reader.ReadBoolean();
        ModConfig.ServerConfig.enableForVehicleRepair = reader.ReadBoolean();
        ModConfig.ServerConfig.onlyStorageCrates = reader.ReadBoolean();
        ModConfig.ServerConfig.pullFromVehicleStorage = reader.ReadBoolean();
        // ModConfig.ServerConfig.pullFromDroneStorage = reader.ReadBoolean();
        ServerUtils.HasServerConfig = true;
#if DEBUG
        if (!LogUtil.IsDebug()) return;
        LogUtil.DebugLog($"ModConfig.ServerConfig.range {ModConfig.ServerConfig.range}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForBlockRepair {ModConfig.ServerConfig.enableForBlockRepair}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForBlockUpgrade {ModConfig.ServerConfig.enableForBlockUpgrade}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForGeneratorRefuel {ModConfig.ServerConfig.enableForGeneratorRefuel}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForItemRepair {ModConfig.ServerConfig.enableForItemRepair}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForReload {ModConfig.ServerConfig.enableForReload}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForVehicleRefuel {ModConfig.ServerConfig.enableForVehicleRefuel}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.enableForVehicleRepair {ModConfig.ServerConfig.enableForVehicleRepair}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.onlyStorageCrates {ModConfig.ServerConfig.onlyStorageCrates}");
        LogUtil.DebugLog($"ModConfig.ServerConfig.pullFromVehicleStorage {ModConfig.ServerConfig.pullFromVehicleStorage}");
#endif
    }

    public override void ProcessPackage(World world, GameManager callbacks) {
        if (LogUtil.IsDebug()) LogUtil.DebugLog("Updated client config to use server settings.");
    }

    // TODO: Update this if new options are being sent (new config version)
    public override int GetLength() {
        const int ushortSize = 2;
        const int floatSize = 4;
        const int boolSize = 1;
        const int boolCount = 9;
        return ushortSize + floatSize + boolCount * boolSize;
    }
}