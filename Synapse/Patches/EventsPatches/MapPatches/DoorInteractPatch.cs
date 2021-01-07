using System;
using HarmonyLib;
using System.Collections.Generic;
using Logger = Synapse.Api.Logger;
using EventHandler = Synapse.Api.Events.EventHandler;
using UnityEngine;
using Synapse.Api;
using System.Linq;
using Mirror;
using Interactables.Interobjects.DoorUtils;

namespace Synapse.Patches.EventsPatches.MapPatches
{
	[HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
	internal static class DoorInteractPatch
	{
		private static bool Prefix(DoorVariant __instance, ReferenceHub ply, byte colliderId)
		{
			try
			{
				if (!NetworkServer.active)
				{
					Debug.LogWarning("[Server] function 'System.Void Interactables.Interobjects.DoorUtils.DoorVariant::ServerInteract(ReferenceHub,System.Byte)' called on client");
					return false;
				}
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
					var keycardacces = false;
					var items = new List<Api.Items.SynapseItem>();
					if (Server.Get.Configs.SynapseConfiguration.RemoteKeyCard)
						items.AddRange(player.Inventory.Items);
					else
						items.Add(player.ItemInHand);

					foreach (var item in items)
					{
						var allow = __instance.RequiredPermissions.CheckPermissions(item.ItemType, player.Hub);

						Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

						if (allow)
						{
							keycardacces = true;
							break;
						}
					}

					if (ply.characterClassManager.CurClass == RoleType.Scp079 || keycardacces)
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
