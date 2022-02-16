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

            var list = new List<SynapsePrimitiveObject>();
            foreach (var primitive in schematic.PrimitiveObjects)
            {
                var obj = new SynapsePrimitiveObject(primitive.PrimitiveType, primitive.Color, primitive.Position, Quaternion.Euler(primitive.Rotation), primitive.Scale);
                list.Add(obj);
                obj.ObjectToy.transform.parent = GameObject.transform;
            }
            PrimitivesChildrens = list;

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

        public IReadOnlyList<SynapsePrimitiveObject> PrimitivesChildrens { get; internal set; }

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
