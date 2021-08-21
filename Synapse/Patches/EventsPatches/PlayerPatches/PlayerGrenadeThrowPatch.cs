using System;
using System.Collections.Generic;
using HarmonyLib;
using Mirror;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;
using Console = System.Console;
using Object = UnityEngine.Object;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
	//TODO: Reimplement PlayerThrowGrenadeEvent
	/*
    [HarmonyPatch(typeof(GrenadeManager), nameof(GrenadeManager._ServerThrowGrenade))]
    internal static class GrenadeThrowCompletePatch
    {
        private static bool Prefix(ref GrenadeManager __instance, ref IEnumerator<float> __result, ref GrenadeSettings settings, ref float forceMultiplier, ref int itemIndex, ref float delay)
        {
	        try
	        {
		        var player = __instance.GetPlayer();
                
		        if (player == null) return true;
		        
		        var item = __instance.hub.inventory.items[itemIndex].GetSynapseItem();
		        var allow = true;
		        
		        SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Initiating, ref allow);
		        SynapseController.Server.Events.Player.InvokePlayerThrowGrenadeEvent(player, item,ref settings, ref forceMultiplier, ref delay, ref allow);
		        
		        __result = ServerThrowGrenadeOverride(__instance, settings, forceMultiplier, itemIndex, delay, !allow);
	        }
	        catch (Exception e)
	        {
				Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerThrowGrenade failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
				throw;
	        }
            
            return false;
        }
        
        public static IEnumerator<float> ServerThrowGrenadeOverride(GrenadeManager __instance, GrenadeSettings settings, float forceMultiplier, int itemIndex, float delay, bool cancelled)
		{
			if (cancelled)
			{
				yield break;
			}
			
			if (itemIndex < 0 || itemIndex >= __instance.hub.inventory.items.Count)
			{
				yield break;
			}
			__instance.hub.weaponManager.scp268.ServerDisable();
			float networkDelay = Mathf.Max(delay - __instance.velocityAuditPeriod, 0f);
			if (networkDelay > 0f)
			{
				ushort i = 0;
				while ((float)i < 50f * networkDelay)
				{
					yield return 0f;
					ushort num = i;
					i = (ushort) (num + 1);
				}
				if (__instance.hub.characterClassManager.CurClass == global::RoleType.Spectator)
				{
					yield break;
				}
			}
			float auditDelay = Mathf.Min(delay, __instance.velocityAuditPeriod);
			Vector3 relativeVelocity;
			if (auditDelay > 0f)
			{
				Transform localTransform = __instance.gameObject.transform;
				Vector3 initialPosition = localTransform.position;
				float initialTime = Time.time;
				ushort i = 0;
				while ((float)i < 50f * auditDelay)
				{
					yield return 0f;
					ushort num = i;
					i = (ushort) (num + 1);
				}
				if (__instance.hub.characterClassManager.CurClass == global::RoleType.Spectator)
				{
					yield break;
				}
				float num2 = Time.time - initialTime;
				relativeVelocity = (localTransform.position - initialPosition) / num2;
			}
			else
			{
				relativeVelocity = Vector3.zero;
			}
			
			var allow = true;
			var item = __instance.hub.inventory.items[itemIndex].GetSynapseItem();
			SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(__instance.GetPlayer(), item, ItemInteractState.Finalizing, ref allow);
			if (!allow)
			{
				yield break;
			}
			
			Grenade component = Object.Instantiate(settings.grenadeInstance).GetComponent<Grenade>();
			component.InitData(__instance, relativeVelocity, __instance.hub.PlayerCameraReference.forward, forceMultiplier);
			NetworkServer.Spawn(component.gameObject);
			item.Destroy();
			if (settings.inventoryID == global::ItemType.SCP018)
			{
				global::Team team = __instance.hub.characterClassManager.CurRole.team;
				if (team == global::Team.CHI || team == global::Team.CDP)
				{
					Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_scp_item_count", 1), false);
				}
			}
			yield break;
		}
        
    }
	*/
}