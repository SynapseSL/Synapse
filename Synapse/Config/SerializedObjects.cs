using Synapse.Api;
using UnityEngine;
using System;
using Synapse.Api.Items;
using System.Collections.Generic;

namespace Synapse.Config
{
    [Serializable]
    public class SerializedMapPoint
    {
        public SerializedMapPoint(string room, float x, float y, float z)
        {
            Room = room;
            X = x;
            Y = y;
            Z = z;
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

    [Serializable]
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

        public Synapse.Api.Items.SynapseItem Parse() => new SynapseItem(ID, Durabillity, Sight, Barrel, Other) { Scale = new Vector3(XSize,YSize,ZSize)};
    }

    [Serializable]
    public class SerializedPlayerItem : SerializedItem
    {
        public SerializedPlayerItem() { }

        public SerializedPlayerItem(Synapse.Api.Items.SynapseItem item, short chance, bool preference) : this(item.ID, item.Durabillity, item.Barrel, item.Sight, item.Other, item.Scale, chance, preference) { }

        public SerializedPlayerItem(int id, float durabillity, int barrel, int sight, int other, Vector3 scale, short chance, bool preference)
        {
            ID = id;
            Durabillity = durabillity;
            Barrel = barrel;
            Sight = sight;
            Other = other;
            XSize = scale.x;
            YSize = scale.y;
            ZSize = scale.z;
            Chance = chance;
            UsePreferences = preference;
        }

        public short Chance { get; set; }
        public bool UsePreferences { get; set; }

        public SynapseItem Apply(Player player)
        {
            var item = new Api.Items.SynapseItem(ID, Durabillity, Sight, Barrel, Other) 
            { 
                Scale = new Vector3(XSize, YSize, ZSize) 
            };

            if (UsePreferences)
            {
                item.Sight = player.GetSightPreference(item.ItemType);
                item.Barrel = player.GetBarrelPreference(item.ItemType);
                item.Other = player.GetOtherPreference(item.ItemType);
            }

            if(UnityEngine.Random.Range(1f,100f) <= Chance)
                item.PickUp(player);

            return item;
        }
    }

    [Serializable]
    public class SerializedAmmo
    {
        public SerializedAmmo() { }

        public SerializedAmmo(uint ammo5, uint ammo7, uint ammo9)
        {
            Ammo5 = ammo5;
            Ammo7 = ammo7;
            Ammo9 = ammo9;
        }

        public uint Ammo5 { get; set; }
        public uint Ammo7 { get; set; }
        public uint Ammo9 { get; set; }

        public void Apply(Player player)
        {
            player.Ammo5 = Ammo5;
            player.Ammo7 = Ammo7;
            player.Ammo9 = Ammo9;
        }
    }

    [Serializable]
    public class SerializedPlayerInventory
    {
        public List<SerializedPlayerItem> Items { get; set; }

        public SerializedAmmo Ammo { get; set; }

        public void Apply(Player player)
        {
            foreach (var item in Items)
                item.Apply(player);

            Ammo.Apply(player);
        }
    }

    [Serializable]
    public class SerializedVector3
    {
        public SerializedVector3(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }

        public SerializedVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SerializedVector3() { }

        public Vector3 Parse() => new Vector3(X, Y, Z);

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
