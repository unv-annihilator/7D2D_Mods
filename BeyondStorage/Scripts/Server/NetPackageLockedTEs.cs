using System.Collections.Generic;
using BeyondStorage.Scripts.ContainerLogic;

// ReSharper disable MemberCanBePrivate.Global
#if DEBUG
using BeyondStorage.Scripts.Common;
#endif

namespace BeyondStorage.Scripts.Server;

public class NetPackageLockedTEs : NetPackage {
    public int EntryCount;
    public int Length = 5;
    public Dictionary<Vector3i, int> LockedTileEntities;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public override void write(PooledBinaryWriter binaryWriter) {
        var lockedTEs = GameManager.Instance.lockedTileEntities;
        base.write(binaryWriter);
        EntryCount = lockedTEs.Count;
        binaryWriter.Write(lockedTEs.Count);
        foreach (var kvp in lockedTEs) {
            var pos = kvp.Key.ToWorldPos();
            StreamUtils.Write(binaryWriter, pos);
            binaryWriter.Write(kvp.Value);
#if DEBUG
            LogUtil.DebugLog($"pos {pos.x} {pos.y} {pos.z}, value {kvp.Value}");
            LogUtil.DebugLog(
                $"id  {kvp.Key.EntityId}; worldPos {kvp.Key.ToWorldPos()}; worldCenterPos {kvp.Key.ToWorldCenterPos()}; {kvp.Key} {GameManager.Instance.World.GetTileEntity(kvp.Key.ToWorldPos())}");
#endif
        }

        RecalcLength();
    }

    public void RecalcLength() {
        // x, y, z
        const int posIntCount = 3;
        // int size
        const int intSize = 4;
        // base length
        Length = 1 + intSize;
        // add the additional size per entry: ((x,y,z) + entityId) * EntryCount
        Length += (posIntCount * intSize + intSize) * EntryCount;
#if DEBUG
        LogUtil.DebugLog($"entryCount {EntryCount}, length {Length}");
#endif
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

        RecalcLength();
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