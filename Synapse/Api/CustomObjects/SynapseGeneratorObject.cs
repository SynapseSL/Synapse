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
        {
            Generator = new Generator(CreateNetworkObject(GeneratorPrefab, pos, rotation, scale));

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        internal SynapseGeneratorObject(SynapseSchematic.GeneratorConfiguration configuration)
        {
            Generator = new Generator(CreateNetworkObject(GeneratorPrefab, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale));
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
            UpdateEveryFrame = configuration.UpdateEveryFrame;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override GameObject GameObject => Generator.GameObject;
        public override NetworkIdentity NetworkIdentity => Generator.generator.netIdentity;
        public override ObjectType Type => ObjectType.Generator;
        public override void Destroy()
        {
            //The Generator will add itself on first Start to Map.Generators
            Map.Get.Generators.Remove(Generator);
            base.Destroy();
        }

        public Generator Generator { get; }
    }
}
