using System;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.Item
{
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
