using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using Logger = Synapse.Api.Logger;

namespace Synapse.Events.Patches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerInteract))]
	internal static class GeneratorInteractPatch
    {
		[HarmonyPrefix]
		private static bool OnInteract(Scp079Generator __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
				var gen = __instance.GetGenerator();
				var player = ply.GetPlayer();

				if (__instance._cooldownStopwatch.IsRunning && __instance._cooldownStopwatch.Elapsed.TotalSeconds < __instance._targetCooldown)
					return false;

				if (colliderId != 0 && !gen.Open) return false;

				__instance._cooldownStopwatch.Stop();

                switch (colliderId)
                {
					case 0:
                        if (!gen.Locked)
                        {
							var allow = true;
							Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, gen, 
								gen.Open ? GeneratorInteraction.CloseDoor : GeneratorInteraction.OpenDoor, ref allow);

							if (!allow) return false;

							gen.Open = !gen.Open;
							__instance._targetCooldown = __instance._doorToggleCooldownTime;
                        }
                        else
                        {
							if (!SynapseExtensions.CanHarmScp(player))
                            {
								__instance.RpcDenied();
								return false;
							}

							var items = new List<SynapseItem>();
							if (player.ItemInHand.ID != -1) items.Add(player.ItemInHand);

							if (Server.Get.Configs.synapseConfiguration.RemoteKeyCard)
								items.AddRange(player.Inventory.Items.Where(x => x != player.ItemInHand));

							var canOpen = false;

							foreach (var item in items.Where(x => x.ItemCategory == ItemCategory.Keycard))
								if ((item.ItemBase as KeycardItem).Permissions.HasFlagFast(__instance._requiredPermission))
                                {
									canOpen = true;
									Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref canOpen);
									break;
								}

							Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, gen, GeneratorInteraction.Unlocked, ref canOpen);

							if (canOpen) gen.Locked = false;
							else __instance.RpcDenied();

							__instance._targetCooldown = __instance._unlockCooldownTime;
                        }
						break;

					case 1:
						if((__instance.Activating || SynapseExtensions.CanHarmScp(player)) && !__instance.Engaged)
                        {
							var allow = true;
							Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, gen,
								gen.Active ? GeneratorInteraction.Disabled : GeneratorInteraction.Activated, ref allow);

							if (!allow) return false;

							__instance.Activating = !__instance.Activating;

							if (__instance.Activating)
								__instance._leverStopwatch.Restart();

							__instance._targetCooldown = __instance._doorToggleCooldownTime;
                        }
						break;

					case 2:
						if (__instance.Activating && !__instance.Engaged)
						{
							var allow = true;
							Server.Get.Events.Player.InvokePlayerGeneratorInteractEvent(player, gen,
								GeneratorInteraction.Disabled, ref allow);

							if (!allow) return false;

							gen.Active = false;
							__instance._targetCooldown = __instance._unlockCooldownTime;
						}
						break;


					default:
						__instance._targetCooldown = 1;
						break;
                }

				__instance._cooldownStopwatch.Restart();

				return false;
            }
			catch(Exception e)
            {
				Logger.Get.Error($"Synapse-Event: PlayerGeneratorInteract event failed!!\n{e}");
				return true;
			}
        }
    }
}
