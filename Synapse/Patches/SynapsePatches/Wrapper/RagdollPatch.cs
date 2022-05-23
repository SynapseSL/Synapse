﻿using HarmonyLib;
using Mirror;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Wrapper
{

	[HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.ServerSpawnRagdoll))]
	internal class RagdollPatch
	{
		[HarmonyPrefix]
		private static bool ServerSpawnRagdoll(ReferenceHub hub, PlayerStatsSystem.DamageHandlerBase handler)
		{
			if (hub is null) return false;

			var model_ragdoll = hub.characterClassManager.CurRole.model_ragdoll;

			if (model_ragdoll is null || !Object.Instantiate(model_ragdoll).TryGetComponent<Ragdoll>(out var ragdoll)) return false;

			ragdoll.NetworkInfo = new(hub, handler, model_ragdoll.transform.localPosition, model_ragdoll.transform.localRotation);
			NetworkServer.Spawn(ragdoll.gameObject);

			Map.Get.Ragdolls.Add(new(ragdoll));
			return false;
		}
	}
}