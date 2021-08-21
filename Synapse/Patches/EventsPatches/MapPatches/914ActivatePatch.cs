using System;
using System.Collections.Generic;
using InventorySystem.Items.Pickups;
using NorthwoodLib.Pools;
using Scp914;
using Synapse.Api;
using Synapse.Api.Items;
using UnityEngine;
using Event = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    internal static class Scp914ActivatePatch
    {
		private static bool UpgradePatch(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
        {
			try
			{
				var objects = HashSetPool<GameObject>.Shared.Rent();
				var upgradeDropped = (mode & Scp914Mode.Dropped) == Scp914Mode.Dropped;
				var upgradeInventory = (mode & Scp914Mode.Inventory) == Scp914Mode.Inventory;
				var heldOnly = upgradeInventory && (mode & Scp914Mode.Held) == Scp914Mode.Held;

				var players = new List<Player>();
				var items = new List<SynapseItem>();

				foreach(var collider in intake)
                {
					var gameObject = collider.transform.root.gameObject;

                    if (objects.Add(gameObject))
                    {
						if (ReferenceHub.TryGetHub(gameObject, out var ply))
							players.Add(ply.GetPlayer());
						else if (gameObject.TryGetComponent<ItemPickupBase>(out var pickup))
							items.Add(pickup.GetSynapseItem());
                    }
                }

				Event.Get.Map.Invoke914Activate(ref players, ref items, ref moveVector, out var allow);

				foreach (var ply in players)
					Scp914Upgrader.ProcessPlayer(ply.Hub, upgradeInventory, heldOnly, moveVector, setting);

				foreach (var item in items)
					Scp914Upgrader.ProcessPickup(item.PickupBase, upgradeDropped, moveVector, setting);

				HashSetPool<GameObject>.Shared.Return(objects);
				return false;
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp914 Activate Event failed!!\n{e}");
				return true;
			}
        }
    }
}
