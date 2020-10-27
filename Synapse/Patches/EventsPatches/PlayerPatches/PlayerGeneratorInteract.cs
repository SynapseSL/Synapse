using System;
using Harmony;
using Mirror;
using Synapse.Api;
using Synapse.Api.Enum;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Events.Patches
{
	
	[HarmonyPatch(typeof(Generator079), nameof(Generator079.Interact))]
	internal static class GeneratorPatche
	{
		private static bool Prefix(Generator079 __instance, GameObject person, PlayerInteract.Generator079Operations command)
		{
			try
			{
				var player = person.GetPlayer();
				var generator = __instance.GetGenerator();

				switch (command)
				{
					case PlayerInteract.Generator079Operations.Tablet:

						if (generator.IsTabletConnected || !generator.Open || __instance._localTime <= 0f || Generator079.mainGenerator.forcedOvercharge)
							return false;

						Inventory component = person.GetComponent<Inventory>();

						using (SyncList<Inventory.SyncItemInfo>.SyncListEnumerator enumerator = component.items.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Inventory.SyncItemInfo syncItemInfo = enumerator.Current;
								if (syncItemInfo.id == ItemType.WeaponManagerTablet)
								{
									bool allow2 = true;
									Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, generator,GeneratorInteraction.TabletInjected, ref allow2);
									if (!allow2) break;

									var item = syncItemInfo.GetSynapseItem();
									generator.ConnectedTablet = item;
									break;
								}
							}
						}
						return false;

					case PlayerInteract.Generator079Operations.Cancel:
						if (!generator.IsTabletConnected) return false;

						bool allow = true;
						Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, generator, GeneratorInteraction.TabledEjected, ref allow);
						return allow;
				}
				return true;
			}
			catch (Exception e)
			{
				Logger.Get.Error($"Synapse-Event: PlayerGenerator failed!!\n{e}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(Generator079), nameof(Generator079.OpenClose))]
	public static class GeneratorDoorPatches
	{
		public static bool Prefix(Generator079 __instance, GameObject person)
		{
            try
            {
				var player = person.GetPlayer();
				var generator = __instance.GetGenerator();

				if (player.VanillaInventory == null || __instance._doorAnimationCooldown > 0f || __instance._deniedCooldown > 0f) return false;

				if (!generator.Locked)
				{
					var allow = true;
					if (!generator.Open)
					{
						Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, generator, GeneratorInteraction.OpenDoor, ref allow);
					}
					else
					{
						Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, generator, GeneratorInteraction.CloseDoor, ref allow);
					}

					if (!allow)
					{
						__instance.RpcDenied();
						return false;
					}

					generator.Open = !generator.Open;
					return false;
				}

				//Unlock The Generator
				var flag = player.Bypass;

				if (player.VanillaInventory.GetItemInHand().id > ItemType.KeycardJanitor)
				{
					var permissions = player.VanillaInventory.GetItemByID(player.VanillaInventory.curItem).permissions;

					foreach (var t in permissions)
						if (t == "ARMORY_LVL_2")
							flag = true;
				}

				Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, generator, GeneratorInteraction.Unlocked, ref flag);

				if (flag)
				{
					generator.Locked = false;
					return false;
				}
				__instance.RpcDenied();

				return false;
			}
			catch(Exception e)
            {
				Logger.Get.Error($"Synapse-Event: DoorInteract failed!!\n{e}");
				return true;
            }
		}
	}
	
}
