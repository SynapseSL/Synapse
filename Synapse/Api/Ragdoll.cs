using Mirror;
using UnityEngine;
using System.Linq;
using PlayerStatsSystem;

namespace Synapse.Api
{
    public class Ragdoll
    {
        internal Ragdoll(global::Ragdoll rag) => ragdoll = rag;

        /*public Ragdoll(RoleType roletype, Vector3 pos, Quaternion rot, Vector3 velocity, PlayerStats.HitInfo info, bool allowRecall, string owner)
        {
            var role = Server.Get.Host.ClassManager.Classes.SafeGet((int)roletype);
            var gameobject = UnityEngine.Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
            NetworkServer.Spawn(gameobject);
            ragdoll = gameobject.GetComponent<global::Ragdoll>();
            ragdoll.Networkowner = new global::Ragdoll.Info("", owner, info, role, 0);
            ragdoll.NetworkallowRecall = allowRecall;
            ragdoll.NetworkPlayerVelo = velocity;
            Map.Get.Ragdolls.Add(this);
        }*/

        public Ragdoll(RoleType roletype, Vector3 pos, Quaternion rot, DamageHandlerBase handler, Player owner)
        {
            Role role = Server.Get.Host.ClassManager.Classes.SafeGet((int) roletype);
            GameObject gameObject = Object.Instantiate(role.model_ragdoll, pos + role.model_offset.position,
                Quaternion.Euler(rot.eulerAngles + role.model_offset.rotation));
            NetworkServer.Spawn(gameObject);
            ragdoll = gameObject.GetComponent<global::Ragdoll>();
            ragdoll.NetworkInfo = new RagdollInfo(owner.Hub, handler, pos, rot);
        }

        private readonly global::Ragdoll ragdoll;

        public GameObject GameObject => ragdoll.gameObject;

        public string Name => ragdoll.name;

        public RoleType RoleType
        {
            get => ragdoll.Info.RoleType;
        }

        public Vector3 Position
        {
            get => ragdoll.transform.position;
            set
            {
                NetworkServer.UnSpawn(GameObject);
                ragdoll.transform.position = value;
                NetworkServer.Spawn(GameObject);
            }
        }

        public Vector3 Scale
        {
            get => ragdoll.transform.localScale;
            set
            {
                NetworkServer.UnSpawn(GameObject);
                ragdoll.transform.localScale = value;
                NetworkServer.Spawn(GameObject);
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
        
        public static Ragdoll CreateRagdoll(RoleType roletype, Vector3 pos, Quaternion rot, DamageHandlerBase handler, Player owner) 
            => new Ragdoll(roletype, pos, rot, handler, owner);
    }
}
