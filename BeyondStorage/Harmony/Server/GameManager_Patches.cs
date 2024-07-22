using System.Collections.Generic;
using System.Reflection;
using BeyondStorage.Scripts.ContainerLogic;
using BeyondStorage.Scripts.Server;
using HarmonyLib;
#if DEBUG
using BeyondStorage.Scripts.Common;
#endif

// ReSharper disable UnusedMember.Local

namespace BeyondStorage.Server;

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatches {
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
        // Skip if we're not a server
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        var newLockedDict = GameManager.Instance.lockedTileEntities;
        var currentCount = newLockedDict.Count;
        // Skip if it was 0 and still is (before filtering)
        if (ContainerUtils.LastLockedCount == 0 && currentCount == 0) return;
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"GameManager.{__originalMethod.Name} LTE possibly changed");
#endif
        Dictionary<Vector3i, int> tempDict = new();
        var foundChange = false;
        // Remove anything not player storage
        foreach (var kvp in newLockedDict) {
            // Skip anything not ITileEntityLootable
            if (!kvp.Key.TryGetSelfOrFeature(out ITileEntityLootable tileEntityLootable)) continue;
            // Skip any lootables not of player storage
            if (!tileEntityLootable.bPlayerStorage) continue;
            var tePos = tileEntityLootable.ToWorldPos();
            // Add current entry to our new dict
            tempDict.Add(tePos, kvp.Value);
            // if (!GameManager.IsDedicatedServer) {
            //     // Client-Server update list -- avoid double looping the dicts
            // }
            // skip if we already found a change
            if (foundChange) continue;
            // try and get the key from current dict
            if (ContainerUtils.LockedTileEntities.TryGetValue(tePos, out var currentValue)) {
                if (currentValue != kvp.Value)
                    foundChange = true; // previous value of key changed
            } else {
                // new key found mark as changed
                foundChange = true;
            }
        }

        var newCount = tempDict.Count;

        // skip if we didn't find any change and the lengths are the same
        if (!foundChange && newCount == ContainerUtils.LastLockedCount) return;
        // skip if the new list is empty and was previously empty (similar to another check above but done post filtering)
        if (ContainerUtils.LastLockedCount == 0 && newCount == 0) return;
#if DEBUG
        if (LogUtil.IsDebug()) LogUtil.DebugLog($"Original Count: {newLockedDict.Count}; Filter Count: {newCount}");
#endif
        // store the last count
        ContainerUtils.LastLockedCount = newCount;
        // Update clients
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(new NetPackageLockedTEs());
        // Update the current list locally (Server/Client-Server)
        ContainerUtils.UpdateLockedTEs(tempDict);
    }
}