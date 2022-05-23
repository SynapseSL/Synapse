using System;
using System.Linq;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using Synapse.Api.Events.SynapseEventArguments;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.MapPatches
{
	[HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
	internal static class DoorInteractPatch
	{
		[HarmonyPrefix]
		private static bool OnInteract(DoorVariant __instance, ReferenceHub ply, byte colliderId)
		{
			try
			{
				if (__instance.ActiveLocks > 0)
				{
					DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);
					if ((!mode.HasFlagFast(DoorLockMode.CanClose) || !mode.HasFlagFast(DoorLockMode.CanOpen)) &&
						(!mode.HasFlagFast(DoorLockMode.ScpOverride) || ply.characterClassManager.CurRole.team is not Team.SCP) &&
						(mode == DoorLockMode.FullLock || (__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanClose)) ||
						(!__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanOpen))))
					{
						__instance.LockBypassDenied(ply, colliderId);
						return false;
					}
				}
				if (__instance.AllowInteracting(ply, colliderId))
				{
					var player = ply.GetPlayer();
					var flag = player.RoleType is RoleType.Scp079 || __instance.RequiredPermissions.CheckPermissions(player.VanillaInventory.CurInstance, ply);
					var cardaccess = false;
					var item = player.ItemInHand;

					if (item.ItemCategory is ItemCategory.Keycard)
						try
						{
							EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Finalizing, ref flag);
						}
						catch (Exception ex)
						{
							Logger.Get.Error($"Synapse-Event: ItemUseDoor Event failed!!\n{ex}");
						}

					if (flag) cardaccess = true;
					else if (Server.Get.Configs.SynapseConfiguration.RemoteKeyCard)
						foreach (var item2 in player.Inventory.Items)
						{
							if (item2 == item || item2.ItemCategory is not ItemCategory.Keycard) continue;
							var allowcard = __instance.RequiredPermissions.CheckPermissions(item2.ItemBase, ply);

							try
							{
								EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item2, ItemInteractState.Finalizing, ref allowcard);
							}
							catch (Exception ex)
							{
								Logger.Get.Error($"Synapse-Event: ItemUseDoor Event failed!!\n{ex}");
							}

							if (allowcard)
							{
								cardaccess = true;
								break;
							}
						}

					try
					{
						EventHandler.Get.Map.InvokeDoorInteractEvent(player, __instance.GetDoor(), ref cardaccess);
					}
					catch (Exception ex)
					{
						Logger.Get.Error($"Synapse-Event: DoorInteract failed!!\n{ex}");
					}

					if (cardaccess)
					{
						__instance.NetworkTargetState = !__instance.TargetState;
						__instance._triggerPlayer = ply;
						return false;
					}
					__instance.PermissionsDenied(ply, colliderId);
					DoorEvents.TriggerAction(__instance, DoorAction.AccessDenied, ply);
				}

				return false;
			}
			catch (Exception e)
			{
				Logger.Get.Error($"Synapse-Event: DoorInteract Patch failed!!\n{e}");
				return true;
			}
		}
	}
}