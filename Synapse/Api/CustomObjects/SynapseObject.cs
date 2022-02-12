using AdminToys;
using Mirror;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseObject
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

        public SynapseObject(PrimitiveType primitiveType,Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            ObjectToy = Object.Instantiate(Prefab, position, rotation);
            NetworkServer.Spawn(ObjectToy.gameObject);
            ObjectToy.NetworkPrimitiveType = primitiveType;
            ObjectToy.NetworkMaterialColor = color;
            ObjectToy.transform.position = position;
            ObjectToy.transform.rotation = rotation;
            ObjectToy.transform.localScale = scale;
            ObjectToy.NetworkScale = scale;
            ObjectToy.gameObject.AddComponent<SynapseScript>();

            if (applyPhyics)
                Rigidbody = ObjectToy.gameObject.AddComponent<Rigidbody>();
        }

        public PrimitiveObjectToy ObjectToy { get; set; }

        public Rigidbody Rigidbody { get; private set; }

        public Color Color
        {
            get => ObjectToy.MaterialColor;
        }

        public PrimitiveType PrimitiveType
        {
            get => ObjectToy.PrimitiveType;
        }

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
    }
}
