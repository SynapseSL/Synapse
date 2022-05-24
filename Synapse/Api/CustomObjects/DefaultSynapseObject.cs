using Mirror;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class DefaultSynapseObject : ISynapseObject
    {
        public abstract GameObject GameObject { get; }
        public abstract ObjectType Type { get; }

        public Dictionary<string, object> ObjectData { get; set; } = new Dictionary<string, object>();
        public List<string> CustomAttributes { get; set; }

        public Vector3 OriginalScale { get; internal set; }
        public SynapseObject Parent { get; internal set; }
        public SynapseItem ItemParent { get; internal set; }

        public virtual Vector3 Position
        {
            get => GameObject.transform.position;
            set => GameObject.transform.position = value;
        }

        public virtual Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set => GameObject.transform.rotation = value;
        }

        public virtual Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set => GameObject.transform.localScale = value;
        }

        public virtual Rigidbody Rigidbody { get; set; }

        public virtual void ApplyPhysics()
        {
            if (GameObject.GetComponent<Rigidbody>() is null)
                Rigidbody = GameObject.AddComponent<Rigidbody>();
        }

        public virtual void RemoveParent()
        {
            GameObject.transform.parent = null;
            if (!Map.Get.SynapseObjects.Contains(this))
                Map.Get.SynapseObjects.Add(this);
        }

        public virtual void Destroy()
            => NetworkServer.Destroy(GameObject);
    }
}
