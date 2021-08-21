using System;
using System.Collections.Generic;
using Synapse.Api;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using UnityEngine;

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

        public static explicit operator MapPoint(SerializedMapPoint point) => point.Parse();
        public static implicit operator SerializedMapPoint(MapPoint point) => new SerializedMapPoint(point);
    }

    [Serializable]
    public class SerializedItem
    {
        public SerializedItem() { }

        public SerializedItem(SynapseItem item)
            : this(item.ID, item.Durabillity, item.WeaponAttachments, item.Scale) { }

        public SerializedItem(int id, float durabillity, uint weaponattachement, Vector3 scale)
        {
            ID = id;
            Durabillity = durabillity;
            WeaponAttachments = weaponattachement;
            XSize = scale.x;
            YSize = scale.y;
            ZSize = scale.z;
        }

        public int ID { get; set; }
        public float Durabillity { get; set; }
        public uint WeaponAttachments { get; set; }
        public float XSize { get; set; }
        public float YSize { get; set; }
        public float ZSize { get; set; }

        public SynapseItem Parse() => new SynapseItem(ID) 
        { 
            Scale = new Vector3(XSize,YSize,ZSize),
            Durabillity = Durabillity,
            WeaponAttachments = WeaponAttachments
        };

        public static explicit operator SynapseItem(SerializedItem item) => item.Parse();
        public static implicit operator SerializedItem(SynapseItem item) => new SerializedItem(item);

        [Obsolete("Please use the Attachments code now")]
        public SerializedItem(int id, float durabillity, int barrel, int sight, int other, Vector3 scale)
        {
            ID = id;
            Durabillity = durabillity;
            XSize = scale.x;
            YSize = scale.y;
            ZSize = scale.z;
        }
    }

    [Serializable]
    public class SerializedPlayerItem : SerializedItem
    {
        public SerializedPlayerItem() { }

        public SerializedPlayerItem(SynapseItem item, short chance, bool preference) 
            : this(item.ID, item.Durabillity, item.WeaponAttachments, item.Scale, chance, preference) { }

        public SerializedPlayerItem(int id, float durabillity, uint weaponattachement, Vector3 scale, short chance, bool preference) 
            : base(id, durabillity, weaponattachement, scale)
        {
            Chance = chance;
            UsePreferences = preference;
        }

        public short Chance { get; set; }
        public bool UsePreferences { get; set; }

        public SynapseItem Apply(Player player)
        {
            var item = Parse();

            if (UsePreferences) item.WeaponAttachments = player.GetPreference(ItemManager.Get.GetBaseType(ID));

            if(UnityEngine.Random.Range(1f,100f) <= Chance)
                item.PickUp(player);

            return item;
        }

        [Obsolete("Use the Attachements Code now")]
        public SerializedPlayerItem(int id, float durabillity, int barrel, int sight, int other, Vector3 scale, short chance, bool preference)
        {
            ID = id;
            Durabillity = durabillity;
            XSize = scale.x;
            YSize = scale.y;
            ZSize = scale.z;
            Chance = chance;
            UsePreferences = preference;
        }
    }

    [Serializable]
    public class SerializedAmmo
    {
        public SerializedAmmo() { }

        [Obsolete("Ammo is stored in ushort since 11.0")]
        public SerializedAmmo(uint ammo5, uint ammo7, uint ammo9)
        {
            Ammo5 = (ushort)ammo5;
            Ammo7 = (ushort)ammo7;
            Ammo9 = (ushort)ammo9;
            Ammo12 = 0;
            Ammo44 = 0;
        }

        public SerializedAmmo(ushort ammo5, ushort ammo7, ushort ammo9, ushort ammo12, ushort ammo44)
        {
            Ammo5 = ammo5;
            Ammo7 = ammo7;
            Ammo9 = ammo9;
            Ammo12 = ammo12;
            Ammo44 = ammo44;
        }

        public ushort Ammo5 { get; set; }
        public ushort Ammo7 { get; set; }
        public ushort Ammo9 { get; set; }
        public ushort Ammo12 { get; set; }
        public ushort Ammo44 { get; set; }

        public void Apply(Player player)
        {
            player.AmmoBox[AmmoType.Ammo556x45] = Ammo5;
            player.AmmoBox[AmmoType.Ammo762x39] = Ammo7;
            player.AmmoBox[AmmoType.Ammo9x19] = Ammo9;
            player.AmmoBox[AmmoType.Ammo12gauge] = Ammo12;
            player.AmmoBox[AmmoType.Ammo44cal] = Ammo44;
        }
    }

    [Serializable]
    public class SerializedPlayerInventory
    {
        public List<SerializedPlayerItem> Items { get; set; }

        public SerializedAmmo Ammo { get; set; }

        public void Apply(Player player)
        {
            player.Inventory.Clear();

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

        public static implicit operator Vector3(SerializedVector3 vector) => vector.Parse();
        public static implicit operator SerializedVector3(Vector3 vector) => new SerializedVector3(vector);
    }
}
