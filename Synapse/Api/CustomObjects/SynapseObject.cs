using AdminToys;
using Mirror;
using System;
using System.Collections.Generic;
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

        public SynapseObject(PrimitiveType primitiveType, Vector3 position) : this(primitiveType, Color.white, position, Quaternion.identity, Vector3.one, false) { }

        public SynapseObject(PrimitiveType primitiveType,Color color, Vector3 position, Quaternion rotation, Vector3 scale, bool applyPhyics)
        {
            ObjectToy = UnityEngine.Object.Instantiate(Prefab, position, rotation);
            NetworkServer.Spawn(ObjectToy.gameObject);
            ObjectToy.NetworkPrimitiveType = primitiveType;
            ObjectToy.NetworkMaterialColor = color;
            ObjectToy.transform.position = position;
            ObjectToy.transform.rotation = rotation;
            ObjectToy.transform.localScale = scale;
            ObjectToy.NetworkScale = scale;
            Script = ObjectToy.gameObject.AddComponent<SynapseScript>();
            Script.SynapseObject = this;
            if (applyPhyics)
                ApplyPhysics();
            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
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

        public void ApplyPhysics() => Rigidbody = ObjectToy.gameObject.AddComponent<Rigidbody>();

        public void Destroy()
        {
            Map.Get.SynapseObjects.Remove(this);
            NetworkServer.Destroy(ObjectToy.gameObject);
        }

        public PrimitiveObjectToy ObjectToy { get; private set; }

        public Rigidbody Rigidbody { get; private set; }

        public SynapseScript Script { get; private set; }

        public Guid Guid { get; } = Guid.NewGuid();

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
