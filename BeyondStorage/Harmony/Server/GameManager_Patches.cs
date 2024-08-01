using System.Collections.Generic;
using System.Reflection;
using BeyondStorage.Scripts.Server;
using BeyondStorage.Scripts.Utils;
using HarmonyLib;

// ReSharper disable UnusedMember.Local

namespace BeyondStorage.Server;

[HarmonyPatch]
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
    private static void Postfix() {
        // Skip if we're not a server
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        // Skip if single player
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer) return;
        // Otherwise update our locked TE list
        ServerUtils.LockedTEsUpdate();
    }
}