using AdminToys;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapsePrimitiveObject : SynapseToyObject<PrimitiveObjectToy>
    {
        public static PrimitiveObjectToy Prefab { get; internal set; }

        public SynapsePrimitiveObject(PrimitiveType primitiveType, Vector3 position) : this(primitiveType, Color.white, position, Quaternion.identity, Vector3.one, false) { }

        public SynapsePrimitiveObject(PrimitiveType primitiveType,Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            ToyBase = CreatePrimitive(primitiveType, color, position, rotation, scale);
            if (applyPhyics) ApplyPhysics();

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        //This constructor is only used for creating Childrens of a Synapse Object
        internal SynapsePrimitiveObject(SynapseSchematic.PrimitiveConfiguration configuration)
        {
            ToyBase = CreatePrimitive(configuration.PrimitiveType, configuration.Color, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
            if (configuration.Physics)
                ApplyPhysics();

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        private PrimitiveObjectToy CreatePrimitive(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var ot = UnityEngine.Object.Instantiate(Prefab, position, rotation);
            NetworkServer.Spawn(ot.gameObject);
            ot.NetworkPrimitiveType = primitiveType;
            ot.NetworkMaterialColor = color;
            ot.transform.position = position;
            ot.transform.rotation = rotation;
            ot.transform.localScale = scale;
            ot.NetworkScale = scale;

            return ot;
        }

        public override ObjectType Type => ObjectType.Primitive;

        public override PrimitiveObjectToy ToyBase { get; }

        public bool IsSolid 
        { 
            get 
            {
                for (int xyz = 0; xyz < 3; xyz++)
                    if (Scale[xyz] < 0)
                        return false;
                return true;
            }
            set
            {
                if (IsSolid != value)
                    Scale = Scale * -1;
            }
        }

        public Color Color
            => ToyBase.MaterialColor;

        public PrimitiveType PrimitiveType
            => ToyBase.PrimitiveType;
    }
}
