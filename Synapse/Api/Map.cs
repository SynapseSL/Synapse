
using System.Collections.Generic;

namespace Synapse.Api
{
    public class Map
    {
        internal Map() { }

        public List<Tesla> Teslas { get; } = new List<Tesla>();

        public List<Elevator> Elevators { get; } = new List<Elevator>();

        public List<Door> Doors { get; } = new List<Door>();

        public List<Room> Rooms { get; } = new List<Room>();
    }
}
