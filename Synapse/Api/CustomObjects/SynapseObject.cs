using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseObject
    {
        public SynapseObject(SynapseSchematic schematic)
        {
            Name = schematic.Name;
            ID = schematic.ID;
            GameObject = new GameObject(Name);

            var list = new List<PrimitiveSynapseObject>();

            foreach (var primitive in schematic.PrimitiveObjects)
            {
                var obj = new PrimitiveSynapseObject(primitive.PrimitiveType, primitive.Color, primitive.Position, Quaternion.Euler(primitive.Rotation), primitive.Scale);
                list.Add(obj);
                obj.ObjectToy.transform.parent = GameObject.transform;
            }

            PrimitiveChildrens = list;
        }

        public Vector3 Position
        {
            get => GameObject.transform.position;
            set => GameObject.transform.position = value;
        }

        public Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set => GameObject.transform.rotation = value;
        }

        public Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                GameObject.transform.localScale = value;
                UpdateScale();
            }
        }

        public GameObject GameObject { get; internal set; }

        public IReadOnlyList<PrimitiveSynapseObject> PrimitiveChildrens { get; internal set; }

        public IReadOnlyList<SynapseObject> Childrens { get; internal set; }

        public string Name { get; }

        public int ID { get; }

        private void UpdateScale()
        {
            foreach (var prim in PrimitiveChildrens)
                prim.ObjectToy.NetworkScale = new Vector3(prim.OriginalScale.x * Scale.x, prim.OriginalScale.y * Scale.y, prim.OriginalScale.z * Scale.z);

            foreach (var child in Childrens)
                child.UpdateScale();
        }
    }
}
