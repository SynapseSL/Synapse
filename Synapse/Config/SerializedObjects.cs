using Synapse.Api;

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

        public MapPoint parse()
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
        //TODO: Seriliazed Item class
    }
}
