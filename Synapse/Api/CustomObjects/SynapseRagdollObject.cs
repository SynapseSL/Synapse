using Mirror;
using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;
using Rag = global::Ragdoll;

namespace Synapse.Api.CustomObjects
{
    public class SynapseRagdollObject : NetworkSynapseObject
    {
        public static Dictionary<RoleType, Rag> Prefabs = new Dictionary<RoleType, Rag>();

        public override GameObject GameObject
            => Ragdoll.GameObject;
        public override NetworkIdentity NetworkIdentity
            => Ragdoll._ragdoll.netIdentity;
        public override ObjectType Type
            => ObjectType.Ragdoll;
        public RoleType RoleType
            => Ragdoll.RoleType;
        public DamageType DamageType
            => Ragdoll._ragdoll.Info.Handler.GetDamageType();
        public string Nick
            => Ragdoll._ragdoll.Info.Nickname;
        public Ragdoll Ragdoll { get; }

        public SynapseRagdollObject(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale, string nick)
        {
            var vanillaRagdoll = CreateNetworkObject(Prefabs[role], pos, rot, scale);
            vanillaRagdoll.NetworkInfo = new RagdollInfo(
                Server.Get.Host,
                damage.GetUniversalDamageHandler(),
                role,
                pos,
                rot,
                nick,
                NetworkTime.time
                );
            Ragdoll = new Ragdoll(vanillaRagdoll);
            Map.Get.Ragdolls.Add(Ragdoll);

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        internal SynapseRagdollObject(SynapseSchematic.RagdollConfiguration configuration)
        {
            var vanillaRagdoll = CreateNetworkObject(Prefabs[configuration.RoleType], configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            vanillaRagdoll.NetworkInfo = new RagdollInfo(
                Server.Get.Host,
                configuration.DamageType.GetUniversalDamageHandler(),
                configuration.RoleType,
                configuration.Position,
                Quaternion.Euler(configuration.Rotation),
                configuration.Nick,
                NetworkTime.time
                );
            Ragdoll = new Ragdoll(vanillaRagdoll);
            Map.Get.Ragdolls.Add(Ragdoll);

            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override void Refresh()
        {
            Ragdoll._ragdoll.NetworkInfo = new RagdollInfo(
                Server.Get.Host,
                DamageType.GetUniversalDamageHandler(),
                RoleType,
                Position,
                Rotation,
                Nick,
                Ragdoll._ragdoll.NetworkInfo.CreationTime
                );
            base.Refresh();
        }
        public override void ApplyPhysics()
        {
            foreach (var rigid in Ragdoll._ragdoll.AllRigidbodies)
                rigid.useGravity = true;
        }
    }
}
