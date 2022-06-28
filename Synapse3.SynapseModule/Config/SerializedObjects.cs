﻿using System;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Config;
//TODO:
/*
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
        public float Durabillity { get; set; } = 0f;
        public uint WeaponAttachments { get; set; } = 0;
        public float XSize { get; set; } = 1f;
        public float YSize { get; set; } = 1f;
        public float ZSize { get; set; } = 1f;

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

        public short Chance { get; set; } = 100;
        public bool UsePreferences { get; set; }

        public SynapseItem Apply(Player player)
        {
            var item = Parse();

            if (UsePreferences && item.ItemCategory == ItemCategory.Firearm) item.WeaponAttachments = player.GetPreference(ItemManager.Get.GetBaseType(ID));

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
    */

    [Serializable]
    public class SerializedAmmo
    {
        public SerializedAmmo() { }

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

        public void Apply(SynapsePlayer player)
        {
            player.Inventory.AmmoBox[AmmoType.Ammo556X45] = Ammo5;
            player.Inventory.AmmoBox[AmmoType.Ammo762X39] = Ammo7;
            player.Inventory.AmmoBox[AmmoType.Ammo9X19] = Ammo9;
            player.Inventory.AmmoBox[AmmoType.Ammo12Gauge] = Ammo12;
            player.Inventory.AmmoBox[AmmoType.Ammo44Cal] = Ammo44;
        }
    }

/*
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
    */

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

        public static implicit operator Vector3(SerializedVector3 vector) => vector?.Parse() ?? Vector3.zero;
        public static implicit operator SerializedVector3(Vector3 vector) => new SerializedVector3(vector);
        public static implicit operator SerializedVector3(Quaternion rotation) => new SerializedVector3(rotation.eulerAngles);
        public static implicit operator Quaternion(SerializedVector3 vector) => Quaternion.Euler(vector);
    }

    [Serializable]
    public class SerializedColor
    {
        public SerializedColor() { }

        public SerializedColor(Color32 color)
        {
            R = color.r / 255f;
            G = color.g / 255f;
            B = color.b / 255f;
            A = color.a / 255f;
        }
        public SerializedColor(Color color)
        {
            R = color.r;
            G = color.g;
            B = color.b;
            A = color.a;
        }
        public SerializedColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; } = 1f;

        public Color Parse() => new Color(R, G, B, A);

        public static implicit operator Color(SerializedColor color) => color.Parse();
        public static implicit operator SerializedColor(Color color) => new SerializedColor(color);
        public static implicit operator Color32(SerializedColor color) => color.Parse();
        public static implicit operator SerializedColor(Color32 color) => new SerializedColor(color);

    }