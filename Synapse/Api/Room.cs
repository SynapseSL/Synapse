using MapGeneration;
using Mirror;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(RoomIdentifier identifier)
        {
            Doors = new List<Door>();
            Cameras = new List<Camera>();

            Identifier = identifier;
            if (Identifier is null)
                Logger.Get.Warn("NULL Room");
            RoomType = Identifier.Name;
            RoomShape = Identifier.Shape;
            GameObject = identifier.gameObject;
            ID = Identifier.UniqueId;

            LightController = GameObject.GetComponentInChildren<FlickerableLightController>();

            foreach (var cam in GameObject.GetComponentsInChildren<Camera079>())
                Cameras.Add(new Camera(cam, this));

            NetworkIdentity = GetNetworkIdentity(RoomType);
        }

        public void LightsOut(float duration)
            => LightController.ServerFlickerLights(duration);

        public void SetLightIntensity(float intensity)
            => LightController.UpdateLightsIntensity(LightController.LightIntensityMultiplier, intensity);

        public GameObject GameObject { get; }

        public RoomIdentifier Identifier { get; }

        public Vector3 Position
        {
            get => GameObject.transform.position;
            set
            {
                if (NetworkIdentity is null)
                    return;
                NetworkIdentity.transform.position = value;
                NetworkIdentity.UpdatePositionRotationScale();
            }
        }

        public Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set
            {
                if (NetworkIdentity is null)
                    return;
                NetworkIdentity.transform.rotation = value;
                NetworkIdentity.UpdatePositionRotationScale();
            }
        }

        public Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                if (NetworkIdentity is null)
                    return;
                NetworkIdentity.transform.localScale = value;
                NetworkIdentity.UpdatePositionRotationScale();
            }
        }

        public string RoomName
            => GameObject.name;

        public NetworkIdentity NetworkIdentity { get; }

        public FlickerableLightController LightController { get; }

        public List<Door> Doors { get; }

        public List<Camera> Cameras { get; }

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

        public Color WarheadColor
        {
            get => LightController.Network_warheadLightColor;
            set => LightController.Network_warheadLightColor = value;
        }

        internal static List<NetworkIdentity> NetworkIdentities { get; set; }

        private static NetworkIdentity GetNetworkIdentity(RoomName room)
        {
            if (NetworkIdentities is null)
                NetworkIdentities = GameObject.FindObjectsOfType<NetworkIdentity>().Where(x => x.name.Contains("All")).ToList();
            return room switch
            {
                MapGeneration.RoomName.Lcz330 => NetworkIdentities.FirstOrDefault(x => x.assetId == new Guid("17f38aa5-1bc8-8bc4-0ad1-fffcbe4214ae")),
                MapGeneration.RoomName.Hcz939 => NetworkIdentities.FirstOrDefault(x => x.assetId == new Guid("d1566564-d477-24c4-c953-c619898e4751")),
                MapGeneration.RoomName.Hcz106 => NetworkIdentities.FirstOrDefault(x => x.assetId == new Guid("c1ae9ee4-cc8e-0794-3b2c-358aa6e57565")),
                _ => null,
            };
        }
    }
}
