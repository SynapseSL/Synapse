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

            foreach(var target in schematic.TargetObjects)
            {
                var obj = new SynapseTargetObject(target);
                TargetChildrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
            }

            foreach(var item in schematic.ItemObjects)
            {
                var obj = new SynapseItemObject(item);
                ItemChildrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
            }

            foreach(var station in schematic.WorkStationObjects)
            {
                var obj = new SynapseWorkStationObject(station);
                WorkStationChildrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
            }

            foreach(var door in schematic.DoorObjects)
            {
                var obj = new SynapseDoorObject(door);
                DoorChildrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
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

        public List<SynapsePrimitiveObject> PrimitivesChildrens { get; } = new List<SynapsePrimitiveObject>();
        public List<SynapseLightObject> LightChildrens { get; } = new List<SynapseLightObject>();
        public List<SynapseTargetObject> TargetChildrens { get; } = new List<SynapseTargetObject>();
        public List<SynapseItemObject> ItemChildrens { get; } = new List<SynapseItemObject>();
        public List<SynapseWorkStationObject> WorkStationChildrens { get; } = new List<SynapseWorkStationObject>();
        public List<SynapseDoorObject> DoorChildrens { get; } = new List<SynapseDoorObject>();

        public string Name { get; }

        public int ID { get; }

        //TODO:
        public override void Destroy()
        {
            foreach (var child in PrimitivesChildrens)
                child.Destroy();
        }

        private void UpdatePositionAndRotation()
        {
            foreach (var station in WorkStationChildrens)
                station.Refresh();

            foreach(var door in DoorChildrens)
                door.Refresh();
        }

        private void UpdateScale()
        {
            foreach (var prim in PrimitivesChildrens)
                prim.Scale = new Vector3(prim.OriginalScale.x * Scale.x, prim.OriginalScale.y * Scale.y, prim.OriginalScale.z * Scale.z);

            foreach(var light in LightChildrens)
                light.Scale = new Vector3(light.OriginalScale.x * Scale.x, light.OriginalScale.y * Scale.y, light.OriginalScale.z * Scale.z);

            foreach (var target in TargetChildrens)
                target.Scale = new Vector3(target.OriginalScale.x * Scale.x, target.OriginalScale.y * Scale.y, target.OriginalScale.z * Scale.z);

            foreach(var item in ItemChildrens)
                item.Scale = new Vector3(item.OriginalScale.x * Scale.x, item.OriginalScale.y * Scale.y, item.OriginalScale.z * Scale.z);

            foreach(var station in WorkStationChildrens)
                station.Scale = new Vector3(station.OriginalScale.x * Scale.x, station.OriginalScale.y * Scale.y, station.OriginalScale.z * Scale.z);

            foreach (var door in DoorChildrens)
                door.Scale = new Vector3(door.OriginalScale.x * Scale.x, door.OriginalScale.y * Scale.y, door.OriginalScale.z * Scale.z);
        }
    }
}
