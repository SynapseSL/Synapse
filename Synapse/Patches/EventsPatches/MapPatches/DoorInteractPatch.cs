﻿using System;
using System.Linq;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
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
					if ((!mode.HasFlagFast(DoorLockMode.CanClose) || !mode.HasFlagFast(DoorLockMode.CanOpen)) && (!mode.HasFlagFast(DoorLockMode.ScpOverride) || ply.characterClassManager.CurRole.team != Team.SCP) && (mode == DoorLockMode.FullLock || (__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanClose)) || (!__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanOpen))))
					{
						__instance.LockBypassDenied(ply, colliderId);
						return false;
					}
				}
				if (__instance.AllowInteracting(ply, colliderId))
				{
					var player = ply.GetPlayer();
					var flag = player.RoleType == RoleType.Scp079 || __instance.RequiredPermissions.CheckPermissions(player.VanillaInventory.CurInstance, ply);
					var cardaccess = false;
					var item = player.ItemInHand;

					if (item.ItemCategory == ItemCategory.Keycard)
						EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref flag);

					if (flag) cardaccess = true;
					else if (Server.Get.Configs.synapseConfiguration.RemoteKeyCard)
						foreach (var item2 in player.Inventory.Items.Where(x => x != item && x.ItemCategory == ItemCategory.Keycard))
						{
							var allowcard = __instance.RequiredPermissions.CheckPermissions(item2.ItemBase, ply);

							EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item2, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allowcard);

							if (allowcard)
							{
								cardaccess = true;
								break;
							}
						}

					EventHandler.Get.Map.InvokeDoorInteractEvent(player, __instance.GetDoor(), ref cardaccess);

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
				Logger.Get.Error($"Synapse-Event: DoorInteract failed!!\n{e}");
				return true;
			}
		}
	}
}
