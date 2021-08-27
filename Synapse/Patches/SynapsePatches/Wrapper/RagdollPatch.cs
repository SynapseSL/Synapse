using HarmonyLib;
using Mirror;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Wrapper
{
    [HarmonyPatch(typeof(RagdollManager),nameof(RagdollManager.SpawnRagdoll))]
    internal class RagdollPatch
    {
		[HarmonyPrefix]
        private static bool SpawnRagdoll(RagdollManager __instance,Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId, bool _096Death)
        {
			var role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
			if (role.model_ragdoll == null)
				return false;

			var gameObject = Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));

			NetworkServer.Spawn(gameObject);

			Ragdoll component = gameObject.GetComponent<Ragdoll>();
			component.Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
			component.NetworkallowRecall = allowRecall;
			component.NetworkPlayerVelo = velocity;
			component.NetworkSCP096Death = _096Death;

			var synapseragdoll = new Synapse.Api.Ragdoll(component);
			Map.Get.Ragdolls.Add(synapseragdoll);
			return false;
        }
    }
}
