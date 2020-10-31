using HarmonyLib;
using Mirror;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Wrapper
{
    [HarmonyPatch(typeof(RagdollManager),nameof(RagdollManager.SpawnRagdoll))]
    internal class RagdollPatch
    {
        private static bool Prefix(RagdollManager __instance,Vector3 pos, Quaternion rot, Vector3 velocity, int classId, global::PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId)
        {
			global::Role role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
			if (role.model_ragdoll == null)
			{
				return false;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
			NetworkServer.Spawn(gameObject);
			global::Ragdoll component = gameObject.GetComponent<global::Ragdoll>();
			if (__instance.cleanupTime > 0)
			{
				component.TimeTillCleanup = __instance.cleanupTime;
			}
			component.Networkowner = new global::Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
			component.NetworkallowRecall = allowRecall;
			component.RpcSyncVelo(velocity);

			var synapseragdoll = new Synapse.Api.Ragdoll(component);
			Map.Get.Ragdolls.Add(synapseragdoll);
			return false;
        }
    }
}
