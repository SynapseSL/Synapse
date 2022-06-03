using System.Collections.Generic;
using MapGeneration;
using Synapse.Api.Enum;
using UnityEngine;
using Mirror;
using System.Linq;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(){}
        
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

            NetworkIdentity = GetNetworkIdentity(RoomType);
        }

        public virtual void LightsOut(float duration)
            => LightController.ServerFlickerLights(duration);

        public virtual void SetLightIntensity(float intensity)
            => LightController.UpdateLightsIntensity(LightController.LightIntensityMultiplier, intensity);

        public virtual GameObject GameObject { get; }

        public RoomIdentifier Identifier { get; }

        public virtual Vector3 Position
        {
            get => GameObject.transform.position;
            set
            {
                if (NetworkIdentity == null) return;
                NetworkIdentity.transform.position = value;
                NetworkIdentity.UpdatePositionRotationScale();
            }
        }

        public virtual Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set
            {
                if (NetworkIdentity == null) return;
                NetworkIdentity.transform.rotation = value;
                NetworkIdentity.UpdatePositionRotationScale();
            }
        }

        public virtual Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                if (NetworkIdentity == null) return;
                NetworkIdentity.transform.localScale = value;
                NetworkIdentity.UpdatePositionRotationScale();
            }
        }

        public virtual string RoomName => GameObject.name;

        public NetworkIdentity NetworkIdentity { get; }

        public FlickerableLightController LightController { get; }

        public virtual List<Door> Doors { get; } = new List<Door>();

        public List<Camera> Cameras { get; } = new List<Camera>();

        public int ID { get; }

        public virtual ZoneType Zone
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

        public virtual Color WarheadColor { get => LightController.Network_warheadLightColor; set => LightController.Network_warheadLightColor = value; }

        internal static List<NetworkIdentity> networkIdentities;

        private static NetworkIdentity GetNetworkIdentity(RoomName room)
        {
            if(networkIdentities == null) networkIdentities = GameObject.FindObjectsOfType<NetworkIdentity>().Where(x => x.name.Contains("All")).ToList();
            switch (room)
            {
                case MapGeneration.RoomName.Lcz330:
                    return networkIdentities.FirstOrDefault(x => x.assetId == new System.Guid("17f38aa5-1bc8-8bc4-0ad1-fffcbe4214ae"));

                case MapGeneration.RoomName.Hcz939:
                    return networkIdentities.FirstOrDefault(x => x.assetId == new System.Guid("d1566564-d477-24c4-c953-c619898e4751"));

                case MapGeneration.RoomName.Hcz106:
                    return networkIdentities.FirstOrDefault(x => x.assetId == new System.Guid("c1ae9ee4-cc8e-0794-3b2c-358aa6e57565"));

                default: return null;
            }
        }
    }
}
