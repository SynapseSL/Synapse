using AdminToys;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapsePrimitiveObject : SynapseToyObject<PrimitiveObjectToy>
    {
        public static PrimitiveObjectToy Prefab { get; set; }

        public SynapsePrimitiveObject(PrimitiveType primitiveType, Vector3 position) : this(primitiveType, Color.white, position, Quaternion.identity, Vector3.one, false) { }

        public SynapsePrimitiveObject(PrimitiveType primitiveType,Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            ToyBase = CreatePrimitive(primitiveType, color, position, rotation, scale);
            if (applyPhyics) ApplyPhysics();

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }

        //This constructor is only used for creating Childrens of a Synapse Object
        internal SynapsePrimitiveObject(SynapseSchematic.PrimitiveConfiguration configuration)
        {
            ToyBase = CreatePrimitive(configuration.PrimitiveType, configuration.Color, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
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

        public Color Color
            => ToyBase.MaterialColor;

        public PrimitiveType PrimitiveType
            => ToyBase.PrimitiveType;
    }
}
