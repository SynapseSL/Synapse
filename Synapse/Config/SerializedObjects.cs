using Synapse.Api;
using UnityEngine;

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

        public MapPoint Parse() => MapPoint.Parse(ToString());

        public override string ToString() => $"{Room}:{X}:{Y}:{Z}";
    }

    public class SerializedItem
    {
        public SerializedItem(Synapse.Api.Items.SynapseItem item)
        {
            ID = item.ID;
            Durabillity = item.Durabillity;
            Barrel = item.Barrel;
            Sight = item.Sight;
            Other = item.Other;
            XSize = item.Scale.x;
            YSize = item.Scale.y;
            ZSize = item.Scale.z;
        }

        public SerializedItem(int id, float durabillity, int barrel, int sight, int other,Vector3 scale)
        {
            ID = id;
            Durabillity = durabillity;
            Barrel = barrel;
            Sight = sight;
            Other = other;
            XSize = scale.x;
            YSize = scale.y;
            ZSize = scale.z;
        }

        public SerializedItem()
        {
        }

        public int ID { get; set; }
        public float Durabillity { get; set; }
        public int Sight { get; set; }
        public int Barrel { get; set; }
        public int Other { get; set; }
        public float XSize { get; set; }
        public float YSize { get; set; }
        public float ZSize { get; set; }

        public Synapse.Api.Items.SynapseItem Parse() => new Api.Items.SynapseItem(ID, Durabillity, Sight, Barrel, Other) { Scale = new Vector3(XSize,YSize,ZSize)};
    }
}
