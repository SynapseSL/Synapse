using Mirror;
using System.Net;
using UnityEngine;

namespace Synapse.Api
{
    public class Ragdoll
    {
        internal Ragdoll(global::Ragdoll rag) => ragdoll = rag;

        public Ragdoll(RoleType roletype, Vector3 pos, Quaternion rot, Vector3 velocity, PlayerStats.HitInfo info, bool allowRecall, Player owner)
        {
            var role = Server.Get.Host.ClassManager.Classes.SafeGet((int)roletype);
            var gameobject = UnityEngine.Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
            NetworkServer.Spawn(gameobject);
            ragdoll = gameobject.GetComponent<global::Ragdoll>();
            ragdoll.Networkowner = new global::Ragdoll.Info(owner.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId, owner.NickName, info,role, owner.PlayerId);
            ragdoll.NetworkallowRecall = allowRecall;
            ragdoll.RpcSyncVelo(velocity);
            Map.Get.Ragdolls.Add(this);
        }

        private readonly global::Ragdoll ragdoll;

        public GameObject GameObject => ragdoll.gameObject;

        public string Name => ragdoll.name;

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
            get => Server.Get.GetPlayer(ragdoll.owner.PlayerId);
            set
            {
                ragdoll.owner.PlayerId = value.PlayerId;
                ragdoll.owner.Nick = value.NickName;
                ragdoll.owner.ownerHLAPI_id = value.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId;
            }
        }

        public bool AllowRecall
        {
            get => ragdoll.allowRecall;
            set => ragdoll.allowRecall = value;
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
            Map.Get.Ragdolls.Remove(this);
        }
    }
}
