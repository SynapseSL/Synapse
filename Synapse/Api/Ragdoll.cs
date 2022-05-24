using Mirror;
using PlayerStatsSystem;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api
{
    public class Ragdoll
    {
        internal Ragdoll(global::Ragdoll rag)
            => _ragdoll = rag;

        public Ragdoll(RoleType roleType, string name, Vector3 pos, Quaternion rot, DamageType type)
            : this(roleType, name, pos, rot, type.GetUniversalDamageHandler()) { }

        public Ragdoll(RoleType roleType, string name, Vector3 pos, Quaternion rot, DamageHandlerBase handler)
        {
            var gameObject = Server.Get.Host.ClassManager.Classes.SafeGet((int)roleType).model_ragdoll;

            if (gameObject is null || !Object.Instantiate(gameObject).TryGetComponent(out _ragdoll))
                return;

            _ragdoll.NetworkInfo = new RagdollInfo(Server.Get.Host.Hub, handler, roleType, pos, rot, name, NetworkTime.time);
            NetworkServer.Spawn(GameObject);

            Map.Get.Ragdolls.Add(this);
        }

        public readonly global::Ragdoll _ragdoll;

        public GameObject GameObject
            => _ragdoll.gameObject;

        public RoleType RoleType
            => _ragdoll.Info.RoleType;

        public Vector3 Scale
        {
            get => _ragdoll.transform.localScale;
            set
            {
                _ragdoll.transform.localScale = value;
                _ragdoll.netIdentity.UpdatePositionRotationScale();
            }
        }

        public Player Owner
            => Server.Get.GetPlayer(_ragdoll.Info.OwnerHub.playerId);

        public void Destroy()
        {
            Object.Destroy(GameObject);
            _ = Map.Get.Ragdolls.Remove(this);
        }

        public static Ragdoll CreateRagdoll(RoleType roletype, string name, Vector3 pos, Quaternion rot, DamageType type)
            => new Ragdoll(roletype, name, pos, rot, type);
    }
}
