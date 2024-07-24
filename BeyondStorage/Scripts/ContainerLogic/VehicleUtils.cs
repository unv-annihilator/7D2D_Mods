using System.Collections.Generic;
using BeyondStorage.Scripts.Common;
using UnityEngine;

namespace BeyondStorage.Scripts.ContainerLogic;

public static class VehicleUtils {
    public static IEnumerable<EntityVehicle> GetAvailableVehicleStorages() {
        // foreach (var vehicle in VehicleManager.Instance.vehiclesActive) {
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        var playerPos = player.position;
        var rangeConfig = BeyondStorage.Config.range;
        foreach (var entity in GameManager.Instance.World.Entities.list) {
            // skip anything not a vehicle
            if (entity is not EntityVehicle vehicle) continue;
            // skip vehicles without storage
            if (!vehicle.hasStorage()) continue;
            // skip vehicles locked for the player
            if (vehicle.IsLockedForLocalPlayer(player)) continue;
            // skip vehicles outside of range
            if (rangeConfig > 0 && Vector3.Distance(playerPos, vehicle.position) > rangeConfig) continue;
            // try and get current slots in bag
            var slots = vehicle.bag?.GetSlots();
            for (var i = 0; i < slots?.Length; i++) {
                // make sure the vehicle has something in it before returning it
                if (slots[i] is null || slots[i].itemValue is null || slots[i].itemValue.ItemClass is null || slots[i].count == 0) continue;
                if (LogUtil.IsDebug()) LogUtil.DebugLog($"[{i}] item {slots[i].itemValue.ItemClass.GetItemName()}; count {slots[i].count}");
                // Try and add the vehicle
                yield return vehicle;
                break;
            }

            // TODO: Handle locked slots?
            if (LogUtil.IsDebug()) LogUtil.DebugLog($"lockedSlots {vehicle.bag?.LockedSlots}");
        }
    }
}