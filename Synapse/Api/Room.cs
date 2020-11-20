using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(GameObject gameObject)
        {
            var info = gameObject.GetComponentInChildren<RoomInformation>();

            if(info != null)
                RoomType = info.CurrentRoomType;

            GameObject = gameObject;
        }

        public GameObject GameObject { get; }

        public Vector3 Position => GameObject.transform.position;

        public string RoomName => GameObject.name;

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
