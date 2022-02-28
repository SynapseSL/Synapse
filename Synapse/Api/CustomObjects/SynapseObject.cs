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
            CustomAttributes = schematic.CustomAttributes;
            GameObject = new GameObject(Name);

            foreach (var primitive in schematic.PrimitiveObjects)
            {
                var obj = new SynapsePrimitiveObject(primitive);
                PrimitivesChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach(var light in schematic.LightObjects)
            {
                var obj = new SynapseLightObject(light);
                LightChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach(var target in schematic.TargetObjects)
            {
                var obj = new SynapseTargetObject(target);
                TargetChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach(var item in schematic.ItemObjects)
            {
                var obj = new SynapseItemObject(item);
                ItemChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach(var station in schematic.WorkStationObjects)
            {
                var obj = new SynapseWorkStationObject(station);
                WorkStationChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach(var door in schematic.DoorObjects)
            {
                var obj = new SynapseDoorObject(door);
                DoorChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var rag in schematic.RagdollObjects)
            {
                var obj = new SynapseRagdollObject(rag);
                RagdollChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                UpdatePositionAndRotation();
            }
        }

        public override Quaternion Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                UpdatePositionAndRotation();
            }
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

        public List<ISynapseObject> Childrens { get; } = new List<ISynapseObject>();
        public List<SynapsePrimitiveObject> PrimitivesChildrens { get; } = new List<SynapsePrimitiveObject>();
        public List<SynapseLightObject> LightChildrens { get; } = new List<SynapseLightObject>();
        public List<SynapseTargetObject> TargetChildrens { get; } = new List<SynapseTargetObject>();
        public List<SynapseItemObject> ItemChildrens { get; } = new List<SynapseItemObject>();
        public List<SynapseWorkStationObject> WorkStationChildrens { get; } = new List<SynapseWorkStationObject>();
        public List<SynapseDoorObject> DoorChildrens { get; } = new List<SynapseDoorObject>();
        public List<SynapseRagdollObject> RagdollChildrens { get; } = new List<SynapseRagdollObject>();

        public string Name { get; }

        public int ID { get; }

        public override void Destroy()
        {
            foreach(var child in Childrens)
                child.Destroy();

            Object.Destroy(GameObject);
        }

        private void UpdatePositionAndRotation()
        {
            foreach (var station in WorkStationChildrens)
                station.Refresh();

            foreach(var door in DoorChildrens)
                door.Refresh();

            foreach (var rag in RagdollChildrens)
                rag.Refresh();
        }

        private void UpdateScale()
        {
            foreach(var ichild in Childrens)
            {
                var child = ichild as DefaultSynapseObject;
                child.Scale = new Vector3(child.OriginalScale.x * Scale.x, child.OriginalScale.y * Scale.y, child.OriginalScale.z * Scale.z);
            }
        }
    }
}
