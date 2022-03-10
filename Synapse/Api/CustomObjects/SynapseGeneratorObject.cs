using MapGeneration.Distributors;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseGeneratorObject : StructureSyncSynapseObject
    {
        public static Scp079Generator GeneratorPrefab { get; internal set; }

        public SynapseGeneratorObject(Vector3 pos, Quaternion rotation, Vector3 scale)
            => Generator = new Generator(CreateNetworkObject(GeneratorPrefab, pos, rotation, scale));

        public override GameObject GameObject => Generator.GameObject;
        public override NetworkIdentity NetworkIdentity => Generator.generator.netIdentity;
        public override ObjectType Type => ObjectType.Generator;

        public Generator Generator { get; }
    }
}
