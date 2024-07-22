using System.Collections.Generic;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.ContainerLogic;

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
            // var clrIdx = kvp.Key.GetClrIdx();
            // binaryWriter.Write(clrIdx);
            var pos = kvp.Key.ToWorldPos();
            StreamUtils.Write(binaryWriter, pos);
            // binaryWriter.Write(pos.x);
            // binaryWriter.Write(pos.y);
            // binaryWriter.Write(pos.z);
            // var pos = kvp.Key.ToWorldPos().ToStringNoBlanks();
            binaryWriter.Write(kvp.Value);
#if DEBUG
            LogUtil.DebugLog($"pos {pos.x} {pos.y} {pos.z}, value {kvp.Value}");
            LogUtil.DebugLog(
                $"id  {kvp.Key.EntityId}; worldPos {kvp.Key.ToWorldPos()}; worldCenterPos {kvp.Key.ToWorldCenterPos()}; {kvp.Key} {GameManager.Instance.World.GetTileEntity(kvp.Key.ToWorldPos())}");
#endif
        }

        RecalcLength();
#if DEBUG
        LogUtil.DebugLog($"entryCount {EntryCount}, length {Length}");
#endif
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
    }

    public override void read(PooledBinaryReader binaryReader) {
        EntryCount = binaryReader.ReadInt32();
        LockedTileEntities = new Dictionary<Vector3i, int>();
        for (var i = 0; i < EntryCount; i++) {
            // var clrIdx = binaryReader.ReadInt32();
            var pos = StreamUtils.ReadVector3i(binaryReader);
            // var tileEntity = GameManager.Instance.World.GetTileEntity(clrIdx, pos);
            // var tePos = tileEntity.ToWorldPos();
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
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"NetPackageLockedTEs: size {Length}; count {EntryCount}");
        ContainerUtils.UpdateLockedTEs(LockedTileEntities);
    }

    public override int GetLength() {
        return Length;
    }
}