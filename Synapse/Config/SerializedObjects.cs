using Synapse.Api;
using Synapse.Api.Items;
using System.ComponentModel;

namespace Synapse.Config
{
    public class SerializedMapPoint
    {
        public SerializedMapPoint(string room, float x, float y, float z)
        {
            this.Room = room;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public SerializedMapPoint(MapPoint point)
        {
            Room = point.Room.RoomName;
            X = point.RelativePosition.x;
            Y = point.RelativePosition.y;
            Z = point.RelativePosition.z;
        }

        public SerializedMapPoint()
        {
        }

        public string Room { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public MapPoint Parse()
        {
            return MapPoint.Parse(ToString());
        }

        public override string ToString()
        {
            return $"{Room}:{X}:{Y}:{Z}";
        }
    }

    public class SerializedItem
    {
        int ID { get; set; }

        float Durabillity { get; set; }

        int Barrel { get; set; }

        int Sight { get; set; }

        int Other { get; set; }

        public SerializedItem(Synapse.Api.Items.Item item) { }

        public SerializedItem(int id, float durabillity,int barrel, int sight,int other)
        {
            ID = id;
            Durabillity = durabillity;
            Barrel = barrel;
            Sight = sight;
            Other = other;
        }

        public SerializedItem() { }

        public Synapse.Api.Items.Item Parse() => new Api.Items.Item(ID, Durabillity, Sight, Barrel, Other);
    }
}
