using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(RoomManager.Room vroom) => VanillaRoom = vroom;

        private RoomManager.Room VanillaRoom;

        public GameObject GameObject => VanillaRoom.roomPrefab;

        public Vector3 Position => VanillaRoom.roomOffset.position;

        public string RoomName => GameObject.name;

        public ZoneType Zone
        {
            get
            {
                //TODO: Adding All Zones
                if (Position.y > -10f && Position.y < 25f)
                    return ZoneType.LCZ;

                else if (Vector3.Distance(Vector3.up * -1998, Position) < 30f)
                    return ZoneType.Pocket;

                else return ZoneType.None;
            }
        }

        /*
         * TODO: RoomTypes
        public ImageGenerator.RoomType RoomType
        {
            get
            {
                if (RoomName.Contains("_T_"))
                    return ImageGenerator.RoomType.RoomT;

                else if () ;

                else 
                    return ImageGenerator.RoomType.
            }
        }
        */
    }
}
