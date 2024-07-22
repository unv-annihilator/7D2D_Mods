using System.Collections.Generic;
using System.Reflection;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.ContainerLogic;
using BeyondStorage.Scripts.Server;
using HarmonyLib;
using UniLinq;

// ReSharper disable UnusedMember.Local

namespace BeyondStorage.Server;

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatches {
    [HarmonyPrepare]
    private static void Prepare(MethodBase original) {
#if DEBUG
        if (original != null)
            LogUtil.DebugLog($"Adding Postfix to {typeof(GameManager)}.{original.Name} | {original}");
#endif
    }

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods() {
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.ChangeBlocks), [typeof(PlatformUserIdentifierAbs), typeof(List<BlockChangeInfo>)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.ClearTileEntityLockForClient), [typeof(int)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.DropContentOfLootContainerServer), [typeof(BlockValue), typeof(Vector3i), typeof(int), typeof(ITileEntityLootable)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.FreeAllTileEntityLocks), []);
        // yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.GetEntityIDForLockedTileEntity), [typeof(TileEntity)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.ResetWindowsAndLocks), [typeof(HashSetLong)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.TELockServer), [typeof(int), typeof(Vector3i), typeof(int), typeof(int), typeof(string)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.TEUnlockServer), [typeof(int), typeof(Vector3i), typeof(int), typeof(bool)]);
    }

    // ReSharper disable once InconsistentNaming
    [HarmonyPostfix]
    private static void Postfix(MethodBase __originalMethod) {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"GameManager.{__originalMethod.Name} LTE possibly changed");
#endif
        // skip if it was 0 and is still 0
        if (ContainerUtils.LastLockedCount == 0 && GameManager.Instance.lockedTileEntities.Count == 0) return;
        // store the last count
        ContainerUtils.LastLockedCount = GameManager.Instance.lockedTileEntities.Count;
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(new NetPackageLockedTEs());
        if (!GameManager.IsDedicatedServer)
            ContainerUtils.UpdateLockedTEs(GameManager.Instance.lockedTileEntities.ToDictionary(kp => kp.Key.ToWorldPos(), kp => kp.Value));
    }
}