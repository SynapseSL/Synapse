using Synapse.Api.Enum;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(GameObject gameObject)
        {
            var info = gameObject.GetComponentInChildren<RoomInformation>();
            RoomType = info.CurrentRoomType;
            GameObject = gameObject;
            LightController = GameObject.GetComponentInChildren<FlickerableLightController>();

            foreach (var cam in GameObject.GetComponentsInChildren<Camera079>())
                Cameras.Add(new Camera(cam,this));
        }

        internal FlickerableLightController LightController { get; set; }

        public void LightsOut(float duration)
            => LightController.ServerFlickerLights(duration);

        public void SetLightIntensity(float intensity)
            => LightController.ServerSetLightIntensity(intensity);

        public GameObject GameObject { get; }

        public Vector3 Position => GameObject.transform.position;

        public string RoomName => GameObject.name;

        public List<Door> Doors { get; } = new List<Door>();

        public List<Camera> Cameras { get; } = new List<Camera>();

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

        public RoomInformation.RoomType RoomType { get; }
    }
}
