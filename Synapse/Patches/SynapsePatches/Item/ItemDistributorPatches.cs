using System;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using Synapse.Api.Items;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.SpawnPickup))]
    internal static class SpawnPickupPatch
    {
        [HarmonyPrefix]
        private static bool SpawnPickup(ItemPickupBase ipb)
        {
            try
            {
                if (ipb is null) return false;
                NetworkServer.Spawn(ipb.gameObject);

                var serial = InventorySystem.Items.ItemSerialGenerator.GenerateNext();

                var info = new PickupSyncInfo
                {
                    ItemId = ipb.Info.ItemId,
                    Position = ipb.transform.position,
                    Rotation = new LowPrecisionQuaternion(ipb.transform.rotation),
                    Serial = serial,
                    Weight = ipb.Info.Weight,
                    Locked = ipb.Info.Locked
                };
                //So for some Reason Mirror just fucks up when spawning the pickups after a door was opened so I have to set both to ensure the Serial is stored - vanilla bug?
                ipb.NetworkInfo = info;
                ipb.Info = info;
                ipb.InfoReceived(default, info);
                new SynapseItem(ipb);
            }
            catch (Exception e)
            {
                Api.Logger.Get.Error($"Synapse-Item: Error while Spawning Pickup:\n{e}");
            }

            return false;
        }
    }
}