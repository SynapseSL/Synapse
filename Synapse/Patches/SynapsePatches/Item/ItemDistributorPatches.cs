using System;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Item
{
	[HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.CreatePickup))]
	internal static class CreatePickUpPatch
    {
		[HarmonyPrefix]
		private static bool CreatePickupPatch(ItemDistributor __instance, ItemType id, Transform t, string triggerDoor)
		{
			try
			{
				if (!InventorySystem.InventoryItemLoader.AvailableItems.TryGetValue(id, out var itemBase))
					return false;

				var itemPickupBase = UnityEngine.Object.Instantiate(itemBase.PickupDropModel, t.position, t.rotation);
				new SynapseItem(itemPickupBase);
				itemPickupBase.Info.ItemId = id;
				itemPickupBase.Info.Weight = itemBase.Weight;
				itemPickupBase.transform.SetParent(t);
				var pickupDistributorTrigger = (object)itemPickupBase as IPickupDistributorTrigger;
				if (pickupDistributorTrigger != null)
					pickupDistributorTrigger.OnDistributed();

				if (string.IsNullOrEmpty(triggerDoor) || !Interactables.Interobjects.DoorUtils.DoorNametagExtension.NamedDoors.TryGetValue(triggerDoor, out var doorNametagExtension))
				{
					ItemDistributor.SpawnPickup(itemPickupBase);
					return false;
				}

				__instance.RegisterUnspawnedObject(doorNametagExtension.TargetDoor, itemPickupBase.gameObject);
				return false;
			}
			catch (Exception e)
			{
				Api.Logger.Get.Error($"Synapse-Items: CreatePickup failed!!\n{e}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(LockerChamber), nameof(LockerChamber.SpawnItem))]
	internal static class LockerSpawnItemPatch
    {
		[HarmonyPrefix]
		private static bool SpawnItemPatch(LockerChamber __instance, ItemType id, int amount)
		{
			try
			{
				if (id == ItemType.None || !InventorySystem.InventoryItemLoader.AvailableItems.TryGetValue(id, out var itemBase))
					return false;

				for (int i = 0; i < amount; i++)
				{
					var itemPickupBase = UnityEngine.Object.Instantiate(itemBase.PickupDropModel, __instance._spawnpoint.position, __instance._spawnpoint.rotation);
					new SynapseItem(itemPickupBase);
					itemPickupBase.transform.SetParent(__instance._spawnpoint);
					itemPickupBase.Info.ItemId = id;
					itemPickupBase.Info.Weight = itemBase.Weight;
					itemPickupBase.Info.Locked = true;
					__instance._content.Add(itemPickupBase);
					if ((object)itemPickupBase is IPickupDistributorTrigger pickupDistributorTrigger)
					{
						pickupDistributorTrigger.OnDistributed();
					}
					if (__instance._spawnOnFirstChamberOpening)
					{
						__instance._toBeSpawned.Add(itemPickupBase);
					}
					else
					{
						ItemDistributor.SpawnPickup(itemPickupBase);
					}
				}

				return false;
			}
			catch (Exception e)
			{
				Api.Logger.Get.Error($"Synapse-Items: Locker SpawnItem failed!!\n{e}");
				return true;
			}
		}
	}
}
