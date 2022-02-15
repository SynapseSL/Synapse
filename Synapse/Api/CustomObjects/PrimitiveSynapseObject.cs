using AdminToys;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class PrimitiveSynapseObject : ISynapseObject
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

        public PrimitiveSynapseObject(PrimitiveType primitiveType, Vector3 position) : this(primitiveType, Color.white, position, Quaternion.identity, Vector3.one, false) { }

        public PrimitiveSynapseObject(PrimitiveType primitiveType,Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            ObjectToy = CreatePrimitive(primitiveType, color, position, rotation, scale, applyPhyics);

            Script = ObjectToy.gameObject.AddComponent<SynapseScript>();
            Script.SynapseObject = this;

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }

        //This constructor is only used for creating Childrens of a Synapse Object
        internal PrimitiveSynapseObject(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ObjectToy = CreatePrimitive(primitiveType, color, position, rotation, scale, false);
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

        public Dictionary<string, object> ObjectData { get; } = new Dictionary<string, object>();

        public Vector3 Position
        {
            get => ObjectToy.transform.position;
            set => ObjectToy.transform.position = value;
        }

        public Quaternion Rotation
        {
            get => ObjectToy.transform.rotation;
            set => ObjectToy.transform.rotation = value;
        }

        public Vector3 Scale
        {
            get => ObjectToy.transform.localScale;
            set
            {
                ObjectToy.transform.localScale = value;
                ObjectToy.NetworkScale = value;
            }
        }

        public bool IsPrimitive => true;

        internal Vector3 OriginalScale { get; set; }

        public void ApplyPhysics() => Rigidbody = ObjectToy.gameObject.AddComponent<Rigidbody>();

        public void Destroy()
        {
            Map.Get.SynapseObjects.Remove(this);
            NetworkServer.Destroy(ObjectToy.gameObject);
        }

        public PrimitiveObjectToy ObjectToy { get; private set; }

        public GameObject GameObject => ObjectToy.gameObject;

        public Rigidbody Rigidbody { get; private set; }

        public SynapseScript Script { get; private set; }

        public bool UsePhysics => Rigidbody != null;

        public Color Color
        {
            get => ObjectToy.MaterialColor;
        }

        public PrimitiveType PrimitiveType
        {
            get => ObjectToy.PrimitiveType;
        }
    }
}
