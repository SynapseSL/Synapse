using System;
using HarmonyLib;
using MEC;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Locker),nameof(Locker.DoorTrigger))]
    internal static class SpawnItemPatch
    {
        private static bool Prefix(Locker __instance)
        {
            try
            {
                if (__instance.SpawnOnOpen) return false;

                for (int i = __instance._itemsToSpawn.Count - 1; i >= 0; i--)
                    SpawnItemToSpawn(__instance._itemsToSpawn[i], __instance, true);

                __instance._itemsToSpawn.Clear();
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: SpawnItem failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }

            return false;
        }

        internal static void SpawnItemToSpawn(Locker.ItemToSpawn toSpawn,Locker locker,bool locked)
        {
            for (int i = 0; i <= toSpawn._amount; i++)
            {
                var item = new Synapse.Api.Items.SynapseItem(toSpawn._id, 0f, 0, 0, 0);
                item.Drop(toSpawn._pos);
                item.pickup.SetupPickup(item.ItemType, item.Durabillity, item.pickup.ownerPlayer, item.pickup.weaponMods, item.pickup.position, toSpawn._rot);
                item.pickup.RefreshDurability(true,true);
                item.pickup.Locked = locked;
                item.pickup.Chamber = toSpawn.chamberId;
                locker.AssignPickup(item.pickup);
                var rb = item.pickup.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                if(locker.enableSorting && locker.sortingTarget != null)
                {
                    rb.useGravity = false;
                    var normalized = (item.Position - locker.sortingTarget.transform.position).normalized;
                    rb.velocity = normalized * locker.sortingForce;
                    rb.angularVelocity = locker.sortingTorque;
                    Timing.CallDelayed(0.5f, () => rb.useGravity = true);
                    return;
                }
                rb.useGravity = true;
            }
        }
    }

    [HarmonyPatch(typeof(Locker), nameof(Locker.LockPickups))]
    internal static class SpawnItemPatch2
    {
        private static bool Prefix(Locker __instance, bool state, uint chamberId, bool anyOpen)
        {
            try
            {
                if(!state && (__instance.SpawnOnOpen || __instance.TriggeredByDoor))
                {
                    __instance.ProcessChambers();
                    for (int i = __instance._itemsToSpawn.Count - 1; i >= 0; i--)
                        if(__instance._itemsToSpawn[i].chamberId == chamberId || __instance.chambers[(int)__instance._itemsToSpawn[i].chamberId].Virtual)
                        {
                            SpawnItemPatch.SpawnItemToSpawn(__instance._itemsToSpawn[i], __instance, false);
                            __instance._itemsToSpawn.RemoveAt(i);
                        }
                }
                if (__instance._assignedPickups == null) return false;

                for (int j = __instance._assignedPickups.Count - 1; j >= 0; j--)
                {
                    if (__instance._assignedPickups[j] == null) __instance._assignedPickups.RemoveAt(j);
                    else
                    {
                        if (__instance._assignedPickups[j].Chamber == chamberId) __instance._assignedPickups[j].Locked = state;
                        if (__instance.chambers[(int)__instance._assignedPickups[j].Chamber].Virtual) __instance._assignedPickups[j].Locked = anyOpen;
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: SpawnItem failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Locker), nameof(Locker.SpawnItem))]
    internal static class SpawnItemPatch3
    {
        private static bool Prefix(Locker __instance, Locker.ItemToSpawn item)
        {
            try
            {
                if (!__instance.Spawned) return false;

                if(__instance.SpawnOnOpen || __instance.TriggeredByDoor)
                {
                    if (__instance._itemsToSpawn == null) __instance._itemsToSpawn = new System.Collections.Generic.List<Locker.ItemToSpawn>();
                    __instance._itemsToSpawn.Add(item);
                    return false;
                }
                SpawnItemPatch.SpawnItemToSpawn(item, __instance, true);
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: SpawnItem failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }

            return false;
        }
    }
}
