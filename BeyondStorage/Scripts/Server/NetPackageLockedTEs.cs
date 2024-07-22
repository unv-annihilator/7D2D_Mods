// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using BeyondStorage.Scripts.ContainerLogic;
#if DEBUG
using BeyondStorage.Scripts.Common;
#endif

namespace BeyondStorage.Scripts.Server;

public class NetPackageLockedTEs : NetPackage {
    public int EntryCount;
    public int Length = 5;
    public Dictionary<Vector3i, int> LockedTileEntities;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public NetPackageLockedTEs Setup(Dictionary<Vector3i, int> lockedTEs) {
        LockedTileEntities = new Dictionary<Vector3i, int>(lockedTEs);
        EntryCount = LockedTileEntities.Count;
        UpdateLength();
        return this;
    }

    public override void write(PooledBinaryWriter binaryWriter) {
        base.write(binaryWriter);
        binaryWriter.Write(LockedTileEntities.Count);
        foreach (var kvp in LockedTileEntities) {
            StreamUtils.Write(binaryWriter, kvp.Key);
            binaryWriter.Write(kvp.Value);
#if DEBUG
            LogUtil.DebugLog($"pos {kvp.Key}, value {kvp.Value}");
#endif
        }
    }

    public void UpdateLength() {
        // x, y, z
        const int posIntCount = 3;
        // int size
        const int intSize = 4;
        // base length
        Length = 1 + intSize;
        // add the additional size per entry: ((x,y,z) + entityId) * EntryCount
        Length += (posIntCount * intSize + intSize) * EntryCount;
    }

    public override void read(PooledBinaryReader binaryReader) {
        EntryCount = binaryReader.ReadInt32();
        LockedTileEntities = new Dictionary<Vector3i, int>();
        for (var i = 0; i < EntryCount; i++) {
            var pos = StreamUtils.ReadVector3i(binaryReader);
            var lockingEntityId = binaryReader.ReadInt32();
#if DEBUG
            LogUtil.DebugLog($"tePOS {pos}; lockingEntityId {lockingEntityId}");
#endif
            LockedTileEntities.Add(pos, lockingEntityId);
        }
#if DEBUG
        var tempLength = Length;
#endif

        UpdateLength();
#if DEBUG
        LogUtil.DebugLog($"count: {EntryCount}; LTE_Dict count {LockedTileEntities.Count}; length {Length}; oldLength {tempLength}");
#endif
    }

    public override void ProcessPackage(World _world, GameManager _callbacks) {
        ContainerUtils.UpdateLockedTEs(LockedTileEntities);
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"NetPackageLockedTEs: size {Length}; count {EntryCount}");
#endif
    }

    public override int GetLength() {
        return Length;
    }
}