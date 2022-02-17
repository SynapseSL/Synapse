using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseObject : DefaultSynapseObject
    {
        public SynapseObject(SynapseSchematic schematic)
        {
            Name = schematic.Name;
            ID = schematic.ID;
            GameObject = new GameObject(Name);

            foreach (var primitive in schematic.PrimitiveObjects)
            {
                var obj = new SynapsePrimitiveObject(primitive);
                PrimitivesChildrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
            }

            foreach(var light in schematic.LightObjects)
            {
                var obj = new SynapseLightObject(light);
                LightChildrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
            }

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }

        public override Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                GameObject.transform.localScale = value;
                UpdateScale();
            }
        }

        public override GameObject GameObject { get; }

        public override ObjectType Type => ObjectType.Shematic;

        public List<SynapsePrimitiveObject> PrimitivesChildrens { get; internal set; } = new List<SynapsePrimitiveObject>();

        public List<SynapseLightObject> LightChildrens { get; internal set; } = new List<SynapseLightObject>();

        public string Name { get; }

        public int ID { get; }

        public override void Destroy()
        {
            foreach (var child in PrimitivesChildrens)
                child.Destroy();
        }

        private void UpdateScale()
        {
            foreach (var prim in PrimitivesChildrens)
                prim.Scale = new Vector3(prim.OriginalScale.x * Scale.x, prim.OriginalScale.y * Scale.y, prim.OriginalScale.z * Scale.z);
        }
    }
}
