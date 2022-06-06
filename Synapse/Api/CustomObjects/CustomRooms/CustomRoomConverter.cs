using System.Collections.Generic;
using System.Linq;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects.CustomRooms
{
    public class CustomRoomConverter : Room
    {
        internal CustomRoomConverter()
            => Map.Get.Rooms.Add(this);
        
        public CustomRoom CustomRoom { get; set; }

        public override Vector3 Position
        {
            get => CustomRoom.Position;
            set => CustomRoom.Position = value;
        }

        public override Quaternion Rotation
        {
            get => CustomRoom.Rotation;
            set => CustomRoom.Rotation = value;
        }

        public override Vector3 Scale
        {
            get => CustomRoom.Scale;
            set => CustomRoom.Scale = value;
        }

        public override GameObject GameObject => CustomRoom.GameObject;

        public override string RoomName => CustomRoom.Name;

        public override List<Door> Doors => CustomRoom.Room.DoorChildrens.Select(x => x.Door).ToList();

        public override void LightsOut(float duration) => CustomRoom.LightsOut(duration);

        public override ZoneType Zone
        {
            get
            {
                switch (CustomRoom.ZoneID)
                {
                    case 0: return ZoneType.None;
                    case 1: return ZoneType.LCZ;
                    case 2: return ZoneType.HCZ;
                    case 3: return ZoneType.Entrance;
                    case 4: return ZoneType.Surface;
                    case 5: return ZoneType.Pocket;
                    default: return ZoneType.Custom;
                }
            }
        }

        public override void SetLightIntensity(float intensity) => CustomRoom.SetLightsIntensity(intensity);

        public override Color WarheadColor
        {
            get => CustomRoom.Room.LightChildrens.FirstOrDefault()?.LightColor ?? Color.white;
            set => CustomRoom.SetLightsColor(value);
        }
    }
}