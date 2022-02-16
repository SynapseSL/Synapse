using AdminToys;
using Mirror;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapsePrimitiveObject : SynapseToyObject
    {
        private static PrimitiveObjectToy Prefab { get; set; }

        internal static void Init()
        {
            foreach(var prefab in NetworkManager.singleton.spawnPrefabs)
                if(prefab.TryGetComponent<PrimitiveObjectToy>(out var pref))
                {
                    Prefab = pref;
                    return;
                }
        }

        public SynapsePrimitiveObject(PrimitiveType primitiveType, Vector3 position) : this(primitiveType, Color.white, position, Quaternion.identity, Vector3.one, false) { }

        public SynapsePrimitiveObject(PrimitiveType primitiveType,Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            ObjectToy = CreatePrimitive(primitiveType, color, position, rotation, scale, applyPhyics);
            ToyBase = ObjectToy;

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }

        //This constructor is only used for creating Childrens of a Synapse Object
        internal SynapsePrimitiveObject(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ObjectToy = CreatePrimitive(primitiveType, color, position, rotation, scale, false);
            ToyBase = ObjectToy;
            OriginalScale = scale;
        }

        private PrimitiveObjectToy CreatePrimitive(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            var ot = UnityEngine.Object.Instantiate(Prefab, position, rotation);
            NetworkServer.Spawn(ot.gameObject);
            ot.NetworkPrimitiveType = primitiveType;
            ot.NetworkMaterialColor = color;
            ot.transform.position = position;
            ot.transform.rotation = rotation;
            ot.transform.localScale = scale;
            ot.NetworkScale = scale;
            if (applyPhyics)
                ApplyPhysics();

            return ot;
        }

        public override GameObject GameObject { get => ObjectToy.gameObject; }

        public override ObjectType Type => ObjectType.Primitive;

        public PrimitiveObjectToy ObjectToy { get; private set; }

        public Color Color
            => ObjectToy.MaterialColor;

        public PrimitiveType PrimitiveType
            => ObjectToy.PrimitiveType;
    }
}
