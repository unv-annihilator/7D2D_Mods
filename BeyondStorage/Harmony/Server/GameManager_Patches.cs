#if DEBUG
using BeyondStorage.Scripts.Common;
#endif
using System.Collections.Generic;
using System.Reflection;
using BeyondStorage.Scripts.ContainerLogic;
using BeyondStorage.Scripts.Server;
using HarmonyLib;
using UniLinq;
using Enumerable = System.Linq.Enumerable;

// ReSharper disable UnusedMember.Local

namespace BeyondStorage.Server;

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatches {
    private readonly static Dictionary<ITileEntity, int> _lastDict = new();

#if DEBUG
    [HarmonyPrepare]
    private static void Prepare(MethodBase original) {
        if (original == null) return;
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Adding Postfix to {typeof(GameManager)}.{original.Name} | {original}");
    }
#endif

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods() {
        // , [typeof(PlatformUserIdentifierAbs), typeof(List<BlockChangeInfo>)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.ChangeBlocks));
        // , [typeof(int)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.ClearTileEntityLockForClient));
        // , [typeof(BlockValue), typeof(Vector3i), typeof(int), typeof(ITileEntityLootable)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.DropContentOfLootContainerServer));
        // , []);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.FreeAllTileEntityLocks));
        // yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.GetEntityIDForLockedTileEntity)); // , [typeof(TileEntity)]);
        // , [typeof(HashSetLong)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.ResetWindowsAndLocks));
        // , [typeof(int), typeof(Vector3i), typeof(int), typeof(int), typeof(string)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.TELockServer));
        // , [typeof(int), typeof(Vector3i), typeof(int), typeof(bool)]);
        yield return AccessTools.Method(typeof(GameManager), nameof(GameManager.TEUnlockServer));
    }

    // ReSharper disable once InconsistentNaming
    [HarmonyPostfix]
    private static void Postfix(MethodBase __originalMethod) {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"GameManager.{__originalMethod.Name} LTE possibly changed");
#endif
        // skip if it was 0 and is still 0
        var newLockedDict = GameManager.Instance.lockedTileEntities;
        var currentCount = newLockedDict.Count;
        // skip if the list is empty and was previously empty
        if (ContainerUtils.LastLockedCount == 0 && currentCount == 0) return;
        // continue to send package if there is any different in locked count
        if (ContainerUtils.LastLockedCount != currentCount) {
            // skip if the lists are the same
            if (Enumerable.SequenceEqual(newLockedDict, _lastDict)) return;
        }

        // store the last count
        ContainerUtils.LastLockedCount = currentCount;
        // clear current last dict
        _lastDict.Clear();
        // store last dict
        newLockedDict.CopyTo(_lastDict);
        // send the package
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(new NetPackageLockedTEs());
        if (!GameManager.IsDedicatedServer) // if client hosted server update local client list as well
            ContainerUtils.UpdateLockedTEs(GameManager.Instance.lockedTileEntities.ToDictionary(kp => kp.Key.ToWorldPos(), kp => kp.Value));
    }
}