using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Utils;

namespace BeyondStorage.Scripts.Server;

// ReSharper disable once ClassNeverInstantiated.Global
public class NetPackageBeyondStorageConfig : NetPackage {
    private const ushort ConfigVersion = 1;

    // TODO: Update number if more options being sent
    private const ushort BoolCount = 9;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public override void write(PooledBinaryWriter binaryWriter) {
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Sending config, version {ConfigVersion}, to client.");
        base.write(binaryWriter);
        binaryWriter.Write(ConfigVersion);
// #if DEBUG
//         // Testing backwards compatibility
//         binaryWriter.Write((ushort)(BoolCount + 5));
// #else
        binaryWriter.Write(BoolCount);
// #endif

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

// #if DEBUG
//         // testing backwards compatibility if we are sending more than expecting to receive (EX: newer config sent by server to client running older mod version)
//         binaryWriter.Write(ModConfig.ClientConfig.pullFromVehicleStorage);
//         binaryWriter.Write(ModConfig.ClientConfig.pullFromVehicleStorage);
//         binaryWriter.Write(ModConfig.ClientConfig.pullFromVehicleStorage);
//         binaryWriter.Write(ModConfig.ClientConfig.pullFromVehicleStorage);
//         binaryWriter.Write(ModConfig.ClientConfig.pullFromVehicleStorage);
// #endif
    }

    public override void read(PooledBinaryReader reader) {
        var configVersion = reader.ReadUInt16();
        var sentBoolCount = reader.ReadUInt16();
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Received config from server. Version {configVersion}; sentBoolCount {sentBoolCount}; localBoolCount {BoolCount}.");
        // check if we got the same, newer, or older version of the config.
        switch (configVersion) {
            case > ConfigVersion:
                LogUtil.Warning("Newer configuration version received from server! You might be missing features present on the server and is advised to use the same version.");
                break;
            case < ConfigVersion:
                // TODO: maybe extract what we can from server settings
                LogUtil.Error(
                    "Older configuration version received from server, failed to sync server settings! Either downgrade client mod to the version on the server OR have the server upgrade to client's mod version.");
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
        // Set HasServerConfig = true
        ServerUtils.HasServerConfig = true;

        if (sentBoolCount > BoolCount) {
            for (var i = 0; i < sentBoolCount - BoolCount; i++) {
                // read/discard remaining booleans if more than expected
                reader.ReadBoolean();
            }
        }

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

    public override int GetLength() {
        //  save room for 6 more bytes (future boolean options)
        const int futureReservedSpace = 6;
        const int ushortSize = 2;
        const int floatSize = 4;
        // Future Space + ConfigVersion + BoolCount + Range + (Bool(1) * Count)
        return futureReservedSpace + ushortSize + ushortSize + floatSize + BoolCount;
    }
}