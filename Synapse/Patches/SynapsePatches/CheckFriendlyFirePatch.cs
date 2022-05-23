using HarmonyLib;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.ThrowableProjectiles;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches
{
	[HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.ExplodeDestructible))]
	internal static class ExplosionGrenadePatch
	{
		[HarmonyPrefix]
		private static bool ExplodeDestructible(IDestructible dest, Footprinting.Footprint attacker, Vector3 pos, ExplosionGrenade setts, out bool __result)
		{
			__result = false;
			try
			{
				if (Physics.Linecast(dest.CenterOfMass, pos, InventorySystem.Items.MicroHID.MicroHIDItem.WallMask))
					return false;
				Vector3 a = dest.CenterOfMass - pos;
				float magnitude = a.magnitude;
				float num = setts._playerDamageOverDistance.Evaluate(magnitude);
				bool flag = ReferenceHub.TryGetHubNetID(dest.NetworkId, out var referenceHub);
				if (flag && referenceHub.characterClassManager.CurRole.team == Team.SCP)
				{
					num *= setts._scpDamageMultiplier;
				}
				Vector3 force = (1f - magnitude / setts._maxRadius) * (a / magnitude) * setts._rigidbodyLiftForce + Vector3.up * setts._rigidbodyLiftForce;
				if (num > 0f && dest.Damage(num, new ExplosionDamageHandler(attacker, force, num, 50), dest.CenterOfMass) && flag)
				{
					float num2 = setts._effectDurationOverDistance.Evaluate(magnitude);
					bool flag2 = attacker.Hub == referenceHub;
					if (num2 > 0f && (flag2 || HitboxIdentity.CheckFriendlyFire(attacker.Hub, referenceHub, false)))
					{
						float minimalDuration = setts._minimalDuration;
						ExplosionGrenade.TriggerEffect<CustomPlayerEffects.Burned>(referenceHub, num2 * setts._burnedDuration, minimalDuration);
						ExplosionGrenade.TriggerEffect<CustomPlayerEffects.Deafened>(referenceHub, num2 * setts._deafenedDuration, minimalDuration);
						ExplosionGrenade.TriggerEffect<CustomPlayerEffects.Concussed>(referenceHub, num2 * setts._concussedDuration, minimalDuration);
					}
					if (!flag2 && attacker.Hub is not null)
					{
						Hitmarker.SendHitmarker(attacker.Hub, 1f);
					}
					referenceHub.inventory.connectionToClient.Send(new GunHitMessage(false, num, pos), 0);
				}

				__result = true;
				return false;
			}
			catch (Exception ex)
			{
				Logger.Get.Error($"Synapse-FF: ExplodeDestructible failed!!\n{ex}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(FlashbangGrenade), nameof(FlashbangGrenade.PlayExplosionEffects))]
	internal static class FlashBangPatch
	{
		[HarmonyPrefix]
		private static bool PlayExplosionEffects(FlashbangGrenade __instance)
		{
			try
			{
				float time = __instance._blindingOverDistance.keys[__instance._blindingOverDistance.length - 1].time;
				float num = time * time;
				foreach (var keyValuePair in ReferenceHub.GetAllHubs())
					if (keyValuePair.Value is not null && (__instance.transform.position - keyValuePair.Value.transform.position).sqrMagnitude <= num &&
						!(keyValuePair.Value == __instance.PreviousOwner.Hub) &&
						HitboxIdentity.CheckFriendlyFire(__instance.PreviousOwner.Hub, keyValuePair.Value, false))
						__instance.ProcessPlayer(keyValuePair.Value);

				return false;
			}
			catch (Exception ex)
			{
				Logger.Get.Error($"Synapse-FF: PlayExplosionFlash failed!!\n{ex}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.Damage))]
	internal static class HitboxIdentityPatch
	{
		[HarmonyPrefix]
		private static bool OnDamage(HitboxIdentity __instance, DamageHandlerBase handler)
		{
			try
			{
				if (handler is AttackerDamageHandler ahandler)
					return SynapseExtensions.GetHarmPermission(ahandler.Attacker.GetPlayer(), __instance.TargetHub.GetPlayer());
			}
			catch (Exception ex)
			{
				Logger.Get.Error($"Synapse-FF: PlayerDamage failed!!\n{ex}");
			}

			return true;
		}
	}
}