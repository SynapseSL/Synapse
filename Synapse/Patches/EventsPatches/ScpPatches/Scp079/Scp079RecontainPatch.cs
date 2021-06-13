using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using Synapse.Api.Enum;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp079
{
	[HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079._Recontain))]
	internal static class Scp079RecontainPatch
	{
		private static bool Prefix(out IEnumerator<float> __result, bool forced)
		{
			__result = OverrideRecontain(forced);
			return false;
		}

		private static IEnumerator<float> OverrideRecontain(bool forced)
		{
			var ev = SynapseController.Server.Events.Scp.Scp079;
			try
			{
				ev.Invoke079RecontainEvent(Recontain079Status.Initialize, out var allow);
				if (!allow) yield break;
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent initializing failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
			}

			PlayerStats ps = PlayerManager.localPlayer.GetComponent<PlayerStats>();
			NineTailedFoxAnnouncer annc = NineTailedFoxAnnouncer.singleton;
			while (annc.queue.Count > 0 || AlphaWarheadController.Host.inProgress)
				yield return float.NegativeInfinity;

			if (!forced)
			{
				Respawning.RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
				{
				"JAM_",
				UnityEngine.Random.Range(0, 70).ToString("000"),
				"_",
				UnityEngine.Random.Range(2, 5),
				" SCP079RECON5"
				}), false, true);
			}
			int j;
			for (int i = 0; i < 2750; i = j + 1)
			{
				yield return float.NegativeInfinity;
				j = i;
			}
			while (annc.queue.Count > 0 || AlphaWarheadController.Host.inProgress)
				yield return float.NegativeInfinity;

			try
			{
				ev.Invoke079RecontainEvent(Recontain079Status.Start, out var allow);
				if (!allow) yield break;
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent start failed!!\n{e}");
			}

			Synapse.Api.Map.Get.HeavyController.Is079Recontained = true;

			Respawning.RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
			{
			"JAM_",
			UnityEngine.Random.Range(0, 70).ToString("000"),
			"_",
			UnityEngine.Random.Range(1, 4),
			" SCP079RECON6"
			}), true, true);
			Respawning.RespawnEffectsController.PlayCassieAnnouncement((Scp079PlayerScript.instances.Count > 0) ? "SCP 0 7 9 SUCCESSFULLY TERMINATED USING GENERATOR RECONTAINMENT SEQUENCE" : "FACILITY IS BACK IN OPERATIONAL MODE", false, true);
			for (int i = 0; i < 350; i = j + 1)
			{
				yield return float.NegativeInfinity;
				j = i;
			}

			Generator079.Generators[0].ServerOvercharge(10f, true);

			HashSet<Interactables.Interobjects.DoorUtils.DoorVariant> lockedDoors = new HashSet<Interactables.Interobjects.DoorUtils.DoorVariant>();

			try
			{
				foreach (var door in Synapse.Api.Map.Get.Doors)
				{
                    if (door.VDoor is Interactables.Interobjects.BasicDoor && door.VDoor.TryGetComponent(out Scp079Interactable scp079Interactable))
                    {
                        var zone = scp079Interactable.currentZonesAndRooms.FirstOrDefault();
                        if (zone == null || zone.currentZone != "HeavyRooms") continue;

                        lockedDoors.Add(door.VDoor);
                        door.VDoor.NetworkTargetState = false;
                        door.VDoor.ServerChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.NoPower, true);
                    }
                }
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent lock Door failed!!\n{e}");
			}

			Recontainer079.isLocked = true;

			foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
				ps.HurtPlayer(new PlayerStats.HitInfo(1000001f, "WORLD", DamageTypes.Recontainment, 0), scp079PlayerScript.gameObject, true);

			for (int i = 0; i < 500; i = j + 1)
			{
				yield return float.NegativeInfinity;
				j = i;
			}

			try
			{
				foreach (Interactables.Interobjects.DoorUtils.DoorVariant doorVariant2 in lockedDoors)
					doorVariant2.ServerChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.NoPower, false);
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent unlock door failed!!\n{e}");
			}

			Recontainer079.isLocked = false;

			try
			{
				ev.Invoke079RecontainEvent(Recontain079Status.Finished, out var allow);
				if (!allow) yield break;
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent finished failed!!\n{e}");
			}

			yield break;
		}
	}
}
