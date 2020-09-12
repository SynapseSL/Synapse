using Synapse.Api.Components;
using System.Collections.Generic;

namespace Synapse.Api
{
    public class Map
    {
        internal Map() { }

        public List<Tesla> Teslas { get; } = new List<Tesla>();

        public List<Elevator> Elevators { get; } = new List<Elevator>();

        public List<Components.Door> Doors { get; } = new List<Components.Door>();

        public List<Room> Rooms { get; } = new List<Room>();
    }
}
