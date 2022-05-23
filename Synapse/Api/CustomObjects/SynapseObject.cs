using Mirror;
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

            foreach (var light in schematic.LightObjects)
            {
                var obj = new SynapseLightObject(light);
                LightChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var target in schematic.TargetObjects)
            {
                var obj = new SynapseTargetObject(target);
                TargetChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var item in schematic.ItemObjects)
            {
                var obj = new SynapseItemObject(item);
                ItemChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var station in schematic.WorkStationObjects)
            {
                var obj = new SynapseWorkStationObject(station);
                WorkStationChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var door in schematic.DoorObjects)
            {
                var obj = new SynapseDoorObject(door);
                DoorChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var custom in schematic.CustomObjects)
            {
                var obj = new SynapseCustomObject(custom);
                CustomChildrens.Add(obj);
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

            foreach (var dummy in schematic.DummyObjects)
            {
                var obj = new SynapseDummyObject(dummy);
                DummyChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var generator in schematic.GeneratorObjects)
            {
                var obj = new SynapseGeneratorObject(generator);
                GeneratorChildrens.Add(obj);
                Childrens.Add(obj);
                obj.GameObject.transform.parent = GameObject.transform;
                obj.Parent = this;
            }

            foreach (var locker in schematic.LockerObjects)
            {
                var obj = new SynapseLockerObject(locker);
                LockerChildrens.Add(obj);
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

        public List<ISynapseObject> Childrens { get; } = new();
        public List<SynapsePrimitiveObject> PrimitivesChildrens { get; } = new();
        public List<SynapseLightObject> LightChildrens { get; } = new();
        public List<SynapseTargetObject> TargetChildrens { get; } = new();
        public List<SynapseItemObject> ItemChildrens { get; } = new();
        public List<SynapseWorkStationObject> WorkStationChildrens { get; } = new();
        public List<SynapseDoorObject> DoorChildrens { get; } = new();
        public List<SynapseCustomObject> CustomChildrens { get; } = new();
        public List<SynapseRagdollObject> RagdollChildrens { get; } = new();
        public List<SynapseDummyObject> DummyChildrens { get; } = new();
        public List<SynapseGeneratorObject> GeneratorChildrens { get; } = new();
        public List<SynapseLockerObject> LockerChildrens { get; } = new();

        public string Name { get; }

        public int ID { get; }

        public void DespawnForOnePlayer(Player player)
        {
            foreach (var child in Childrens)
                if (child.GameObject.TryGetComponent<NetworkIdentity>(out var net))
                    net.DespawnForOnePlayer(player);
        }

        public override void Destroy()
        {
            foreach (var child in Childrens)
                child.Destroy();

            Object.Destroy(GameObject);
        }

        private void UpdatePositionAndRotation()
        {
            foreach (var child in Childrens)
                if (child is IRefreshable refresh)
                    refresh.Refresh();
        }

        private void UpdateScale()
        {
            foreach (var ichild in Childrens)
            {
                var child = ichild as DefaultSynapseObject;
                child.Scale = new Vector3(child.OriginalScale.x * Scale.x, child.OriginalScale.y * Scale.y, child.OriginalScale.z * Scale.z);
            }
        }
    }
}