using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse.Api.CustomObjects.CustomRooms
{
    public class CustomRoomHandler
    {
        public static CustomRoomHandler Get => Server.Get.RoomHandler;
        
        internal CustomRoomHandler()
        {
            RegisterCustomRoom<DefaultRoom>();
        }

        public List<CustomRoomInformation> CustomRooms { get; } = new List<CustomRoomInformation>();

        public void RegisterCustomRoom<TRoom>() where TRoom : CustomRoom
        {
            var room = (CustomRoom)Activator.CreateInstance(typeof(TRoom));
            var info = new CustomRoomInformation()
            {
                Name = room.Name,
                ID = room.ID,
                RoomType = typeof(TRoom)
            };

            RegisterCustomRoom(info);
        }

        public void RegisterCustomRoom(CustomRoomInformation info)
        {
            if(IsIDRegistered(info.ID)) return;
            
            CustomRooms.Add(info);
        }

        public void RemoveCustomRoom(int id)
        {
            if(!IsIDRegistered(id)) return;
            var room = CustomRooms.First(x => x.ID == id);
            CustomRooms.Remove(room);
        }

        public CustomRoom GetCustomRoom(int id)
        {
            if (!IsIDRegistered(id)) return null;
            var info = CustomRooms.First(x => x.ID == id);

            if (info.RoomType.GetConstructors().Any(x =>
                    x.GetParameters().Length == 1 && x.GetParameters().First().ParameterType == typeof(int)))
                return (CustomRoom)Activator.CreateInstance(info.RoomType, new object[] { id });
            
            return (CustomRoom)Activator.CreateInstance(info.RoomType);
        }

        public bool IsIDRegistered(int id) => CustomRooms.Any(x => x.ID == id);

        public bool SpawnCustomRoom(int id, Vector3 position)
        {
            if (!IsIDRegistered(id)) return false;

            var room = GetCustomRoom(id);
            room.Generate(position);
            return true;
        }
    }
}