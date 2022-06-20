﻿using Mirror;
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
            Ragdoll = CreateRagDoll(role, damage, pos, rot, scale, nick);

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        internal SynapseRagdollObject(SynapseSchematic.RagdollConfiguration configuration)
        {
            Ragdoll = CreateRagDoll(configuration.RoleType, configuration.DamageType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale, configuration.Nick);
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
        
        public override void Destroy()
        {
            Map.Get.Ragdolls.Remove(Ragdoll);
            base.Destroy();
        }
        
        public Ragdoll CreateRagDoll(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale, string nick)
        {
            var rag = CreateNetworkObject(Prefabs[role], pos, rot, scale);
            rag.NetworkInfo = new RagdollInfo(Server.Get.Host, damage.GetUniversalDamageHandler(), role, pos, rot, nick, NetworkTime.time);

            var srag = new Ragdoll(rag);
            Map.Get.Ragdolls.Add(srag);
            return srag;
        }
    }
}
