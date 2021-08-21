using System.Collections.Generic;
using MapGeneration;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(RoomIdentifier identifier)
        {
            Identifier = identifier;
            if (Identifier == null) Logger.Get.Warn("NULL Room");
            RoomType = Identifier.Name;
            RoomShape = Identifier.Shape;
            GameObject = identifier.gameObject;
            ID = Identifier.UniqueId;

            LightController = GameObject.GetComponentInChildren<FlickerableLightController>();

            foreach (var cam in GameObject.GetComponentsInChildren<Camera079>())
                Cameras.Add(new Camera(cam,this));
        }

        public void LightsOut(float duration)
            => LightController.ServerFlickerLights(duration);

        public void SetLightIntensity(float intensity)
            => LightController.UpdateLightsIntensity(LightController.LightIntensityMultiplier, intensity);

        public GameObject GameObject { get; }

        public RoomIdentifier Identifier { get; }

        public Vector3 Position => GameObject.transform.position;

        public string RoomName => GameObject.name;

        public FlickerableLightController LightController { get; }

        public List<Door> Doors { get; } = new List<Door>();

        public List<Camera> Cameras { get; } = new List<Camera>();

        public int ID { get; }

        public ZoneType Zone
        {
            get
            {
                switch (Position.y)
                {
                    case 0f:
                        return ZoneType.LCZ;

                    case 1000f:
                        return ZoneType.Surface;

                    case -1000f:
                        if (RoomName.Contains("HCZ"))
                            return ZoneType.HCZ;

                        return ZoneType.Entrance;

                    case -2000f:
                        return ZoneType.Pocket;

                    default:
                        return ZoneType.None;
                }
            }
        }

        public RoomName RoomType { get; }

        public RoomShape RoomShape { get; }

        public Color WarheadColor { get => LightController.Network_warheadLightColor; set => LightController.Network_warheadLightColor = value; }
    }
}
