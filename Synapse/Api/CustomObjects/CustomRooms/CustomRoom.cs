using UnityEngine;

namespace Synapse.Api.CustomObjects.CustomRooms
{
    public abstract class CustomRoom
    {
        public SynapseObject Room { get; private set; }

        public CustomRoomConverter Converter { get; private set; }

        public GameObject GameObject => Room.GameObject;
        
        public Vector3 Position
        {
            get => Room.Position;
            set => Room.Position = value;
        }
        
        public Quaternion Rotation
        {
            get => Room.Rotation;
            set => Room.Rotation = value;
        }

        public Vector3 Scale
        {
            get => Room.Scale;
            set => Room.Scale = value;
        }
        
        public abstract string Name { get; }
        
        public abstract int ID { get; }
        
        public abstract int ZoneID { get; }
        
        public abstract int SchematicID { get; }

        public virtual void OnGenerate() { }
        
        public virtual void OnDespawn() { }

        public void Generate(Vector3 position)
        {
            Room = SchematicHandler.Get.SpawnSchematic(SchematicID, position);
            Converter = new CustomRoomConverter()
            {
                CustomRoom = this
            };
            OnGenerate();
        }

        public void Despawn()
        {
            Room?.Destroy();
            if (Converter != null) Map.Get.Rooms.Remove(Converter);
            OnDespawn();
        }

        public void SetLights(bool turnOn = false)
        {
            foreach (var light in Room.LightChildrens)
                light.Enabled = turnOn;
        }

        public void LightsOut(float duration)
        {
            foreach (var light in Room.LightChildrens)
                light.Enabled = false;

            MEC.Timing.CallDelayed(duration, () =>
            {
                foreach (var light in Room.LightChildrens)
                    light.Enabled = true;
            });
        }

        public void SetLightsIntensity(float intensity)
        {
            foreach (var light in Room.LightChildrens)
                light.LightIntensity = intensity;
        }

        public void SetLightsColor(Color color)
        {
            foreach (var light in Room.LightChildrens)
                light.LightColor = color;
        }
    }
}