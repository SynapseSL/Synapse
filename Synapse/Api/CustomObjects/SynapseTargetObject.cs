using AdminToys;
using Mirror;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseTargetObject : SynapseToyObject<ShootingTarget>
    {
        internal static Dictionary<TargetType, ShootingTarget> Prefabs { get; set; } = new Dictionary<TargetType, ShootingTarget>();

        public SynapseTargetObject(TargetType type, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ToyBase = CreateTarget(type, position, rotation, scale);
            TargetType = type;

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }

        internal SynapseTargetObject(SynapseSchematic.TargetConfiguration configuration)
        {
            ToyBase = CreateTarget(configuration.TargetType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
            TargetType = configuration.TargetType;
        }

        public override ObjectType Type => ObjectType.Target;
        public override ShootingTarget ToyBase { get; }

        public TargetType TargetType { get; }

        private ShootingTarget CreateTarget(TargetType type, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var ot = UnityEngine.Object.Instantiate(Prefabs[type], position, rotation);

            ot.transform.position = position;
            ot.transform.rotation = rotation;
            ot.transform.localScale = scale;
            ot.NetworkScale = scale;

            NetworkServer.Spawn(ot.gameObject);
            return ot;
        }
    }
}
