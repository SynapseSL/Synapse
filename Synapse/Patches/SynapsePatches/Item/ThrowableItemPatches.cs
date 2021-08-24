using System;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(ThrowableItem),nameof(ThrowableItem.OnRemoved))]
    internal static class OnRemovedPatch
    {
        [HarmonyPrefix]
        private static bool OnRemoved(ThrowableItem __instance, ItemPickupBase pickup)
        {
            try
            {
                if (pickup == null) return false;
                if (__instance._alreadyFired
                    || __instance.ActivationStopwatch.Elapsed.TotalSeconds < __instance._pinPullTime) return false;

                __instance.ServerThrow(0f, 0f, UnityEngine.Vector3.zero);
                __instance.GetSynapseItem().DespawnPickup();

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: ThrowableItem.OnRemoved Patch failed:\n{e}");
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(ThrowableItem),nameof(ThrowableItem.UpdateServer))]
    internal static class UpdateServerPatch
    {
        [HarmonyPrefix]
        private static bool UpdateServer(ThrowableItem __instance)
        {
            try
            {
                if (__instance._destroyTime != 0 && Time.timeSinceLevelLoad >= __instance._destroyTime)
                    __instance.GetSynapseItem().DespawnItemBase();

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: ThrowableItem.UpdateServer Patch failed:\n{e}");
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(ThrowableItem),nameof(ThrowableItem.ServerThrow),new[] { typeof(float), typeof(float), typeof(Vector3) })]
    internal static class ServerThrowPatch
    {
        [HarmonyPrefix]
        private static bool ServerThrow(ThrowableItem __instance, float forceAmount, float upwardFactor, Vector3 torque)
        {
            try
            {
                __instance._destroyTime = Time.timeSinceLevelLoad + __instance._postThrownAnimationTime;
                __instance._alreadyFired = true;
                var newpickup = UnityEngine.Object.Instantiate<ThrownProjectile>(__instance.Projectile
                    , __instance.Owner.PlayerCameraReference.position, __instance.Owner.PlayerCameraReference.rotation);

                var info = new PickupSyncInfo
                {
                    ItemId = __instance.ItemTypeId,
                    Locked = !__instance._repickupable,
                    Serial = __instance.ItemSerial,
                    Weight = __instance.Weight,
                    Position = newpickup.transform.position,
                    Rotation = new LowPrecisionQuaternion(newpickup.transform.rotation),
                };

                newpickup.NetworkInfo = info;
                newpickup.PreviousOwner = new Footprinting.Footprint(__instance.Owner);
                NetworkServer.Spawn(newpickup.gameObject);
                newpickup.InfoReceived(default, info);
                if (newpickup.TryGetComponent<Rigidbody>(out var rb))
                    __instance.PropelBody(rb, torque, forceAmount, upwardFactor);

                __instance.GetSynapseItem().Throwable.ThrowableItem = newpickup;
                newpickup.ServerActivate();

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: ThrowableItem.ServerThrow Patch failed:\n{e}");
                return true;
            }
        }
    }
}
