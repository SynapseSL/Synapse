using Mirror;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseLockerObject : StructureSyncSynapseObject
    {
        public static Dictionary<LockerType, MapGeneration.Distributors.Locker> Prefabs = new Dictionary<LockerType, MapGeneration.Distributors.Locker>();

        public SynapseLockerObject(LockerType lockerType, Vector3 pos, Quaternion rotation, Vector3 scale)
            => Locker = new Locker(CreateNetworkObject(Prefabs[lockerType], pos, rotation, scale));

        public override NetworkIdentity NetworkIdentity => throw new NotImplementedException();
        public override GameObject GameObject => throw new NotImplementedException();
        public override ObjectType Type => ObjectType.Locker;

        public Locker Locker { get; }
    }
}
