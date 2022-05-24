using Mirror;
using PlayerStatsSystem;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api
{
    public class Ragdoll
    {
        internal Ragdoll(global::Ragdoll rag) => ragdoll = rag;

        public Ragdoll(RoleType roleType, string name, Vector3 pos, Quaternion rot, DamageType type)
            : this(roleType, name, pos, rot, type.GetUniversalDamageHandler()) { }

        public Ragdoll(RoleType roleType, string name, Vector3 pos, Quaternion rot, DamageHandlerBase handler)
        {
            var gameObject = Server.Get.Host.ClassManager.Classes.SafeGet((int) roleType).model_ragdoll;

            if (gameObject is null || !Object.Instantiate(gameObject).TryGetComponent(out ragdoll))
                return;

            ragdoll.NetworkInfo = new RagdollInfo(Server.Get.Host.Hub, handler, roleType, pos, rot, name, NetworkTime.time);
            NetworkServer.Spawn(GameObject);

            Map.Get.Ragdolls.Add(this);
        }

        public readonly global::Ragdoll ragdoll;

        public GameObject GameObject => ragdoll.gameObject;

        public RoleType RoleType
        {
            get => ragdoll.Info.RoleType;
        }

        public Vector3 Scale
        {
            get => ragdoll.transform.localScale;
            set
            {
                ragdoll.transform.localScale = value;
                ragdoll.netIdentity.UpdatePositionRotationScale();
            }
        }

        public Player Owner
        {
            get => Server.Get.GetPlayer(ragdoll.Info.OwnerHub.playerId);
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
            Map.Get.Ragdolls.Remove(this);
        }
        
        public static Ragdoll CreateRagdoll(RoleType roletype,string name, Vector3 pos, Quaternion rot, DamageType type) 
            => new Ragdoll(roletype,name, pos, rot, type);
    }
}
