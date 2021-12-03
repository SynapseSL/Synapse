using HarmonyLib;
using Mirror;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Wrapper
{
	// Should be useless now, but I let it stay for safe keeping
	// Remove when done
	
	// [HarmonyPatch(typeof(RagdollManager),nameof(RagdollManager.SpawnRagdoll))]
 //    internal class RagdollPatchOLD
 //    {
	// 	[HarmonyPrefix]
 //        private static bool SpawnRagdoll(RagdollManager __instance,Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId, bool _096Death)
 //        {
	// 		var role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
	// 		if (role.model_ragdoll == null)
	// 			return false;
 //  
	// 		var gameObject = Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
 //  
	// 		NetworkServer.Spawn(gameObject);
 //  
	// 		Ragdoll component = gameObject.GetComponent<Ragdoll>();
	// 		component.Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
	// 		component.NetworkallowRecall = allowRecall;
	// 		component.NetworkPlayerVelo = velocity;
	// 		component.NetworkSCP096Death = _096Death;
 //  
	// 		var synapseragdoll = new Synapse.Api.Ragdoll(component);
	// 		Map.Get.Ragdolls.Add(synapseragdoll);
	// 		return false;
 //        }
    // }

    [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.ServerSpawnRagdoll))]
    internal class RagdollPatch
    {
	    private static bool ServerSpawnRagdoll(ReferenceHub hub, PlayerStatsSystem.DamageHandlerBase handler)
	    {
		    if (!NetworkServer.active || hub == null) return false;

		    GameObject model_ragdoll = hub.characterClassManager.CurRole.model_ragdoll;
		    Ragdoll ragdoll;
		    if (model_ragdoll == null || !Object.Instantiate<GameObject>(model_ragdoll).TryGetComponent<Ragdoll>(out ragdoll)) return false;

		    ragdoll.NetworkInfo = new RagdollInfo(hub, handler, model_ragdoll.transform.localPosition, model_ragdoll.transform.localRotation);
		    NetworkServer.Spawn(ragdoll.gameObject);
		    
		    Map.Get.Ragdolls.Add(new Synapse.Api.Ragdoll(ragdoll));

		    return true;
	    }
    }
}
