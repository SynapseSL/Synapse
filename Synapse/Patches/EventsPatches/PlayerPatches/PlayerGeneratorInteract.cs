using System;
using HarmonyLib;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

						if (generator.IsTabletConnected || !generator.Open || __instance._localTime <= 0f || Generator079.mainGenerator.forcedOvercharge || !SynapseExtensions.CanHarmScp(player))
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
									var item = syncItemInfo.GetSynapseItem();
									Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, generator,GeneratorInteraction.TabletInjected, ref allow2);
									Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow2);
									if (!allow2) break;

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
				Logger.Get.Error($"Synapse-Event: PlayerGenerator failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
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

                if (!SynapseExtensions.CanHarmScp(player))
                {
					__instance.RpcDenied();
					return false;
                }

				//Unlock The Generator
				var flag = player.Bypass;

				var items = new List<Synapse.Api.Items.SynapseItem>();
				if (Server.Get.Configs.synapseConfiguration.RemoteKeyCard)
					items.AddRange(player.Inventory.Items.Where(x => x.ItemCategory == ItemCategory.Keycard));
				else if (player.ItemInHand != null && player.ItemInHand.ItemCategory == ItemCategory.Keycard)
					items.Add(player.ItemInHand);


				foreach(var item in items)
                {
					var keycardcanopen = false;
					var permissions = player.VanillaInventory.GetItemByID(item.ItemType).permissions;

					foreach (var t in permissions)
						if (t == "ARMORY_LVL_2")
							keycardcanopen = true;

					try
					{
						Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref keycardcanopen);
					}
					catch (Exception e)
					{
						Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent(Keycard) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
					}

					if (keycardcanopen)
                    {
						flag = true;
						break;
					}
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
				Logger.Get.Error($"Synapse-Event: DoorInteract failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
				return true;
            }
		}
	}
	
}
