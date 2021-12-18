using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(ThrowableItem),nameof(ThrowableItem.ServerThrow),new[] { typeof(float), typeof(float), typeof(Vector3),typeof(Vector3) })]
    internal static class ServerThrowPatch
    {
        [HarmonyPrefix]
        private static bool ServerThrow(ThrowableItem __instance, float forceAmount, float upwardFactor, Vector3 torque, Vector3 startVel)
        {
            try
            {
                __instance._destroyTime = Time.timeSinceLevelLoad + __instance._postThrownAnimationTime;
                __instance._alreadyFired = true;
                var newpickup = UnityEngine.Object.Instantiate(__instance.Projectile
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
                    __instance.PropelBody(rb, torque, startVel, forceAmount, upwardFactor);

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

    [HarmonyPatch(typeof(TimedGrenadePickup),nameof(TimedGrenadePickup.Update))]
    internal static class UpdateTimedGrenadePatch
    {
        [HarmonyPrefix]
        private static bool Update(TimedGrenadePickup __instance)
        {
            try
            {
                if (!__instance._replaceNextFrame)
                    return false;

                if (!InventoryItemLoader.AvailableItems.TryGetValue(__instance.Info.ItemId, out var itemBase))
                    return false;

                if (!(itemBase is ThrowableItem throwableItem))
                {
                    return false;
                }
                var thrownProjectile = UnityEngine.Object.Instantiate(throwableItem.Projectile);
                if (thrownProjectile.TryGetComponent<Rigidbody>(out var rigidbody))
                {
                    rigidbody.position = __instance.Rb.position;
                    rigidbody.rotation = __instance.Rb.rotation;
                    rigidbody.velocity = __instance.Rb.velocity;
                    rigidbody.angularVelocity = rigidbody.angularVelocity;
                }
                __instance.Info.Locked = true;
                thrownProjectile.NetworkInfo = __instance.Info;
                thrownProjectile.PreviousOwner = __instance._attacker;
                NetworkServer.Spawn(thrownProjectile.gameObject);
                thrownProjectile.InfoReceived(default, __instance.Info);

                var item = __instance.GetSynapseItem();
                item.Throwable.ThrowableItem = thrownProjectile;

                thrownProjectile.ServerActivate();
                item.DespawnPickup();
                __instance._replaceNextFrame = false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: TimedGrenade Update Patch failed:\n{e}");
            }

            return false;
        }
    }
}
