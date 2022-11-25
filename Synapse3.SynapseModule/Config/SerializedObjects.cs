using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// A Serialized SynapseItem that can be used for configuration files
/// </summary>
[Serializable]
public class SerializedItem
{
    public SerializedItem() { }

    public SerializedItem(SynapseItem item)
        : this(item.Id, item.Durability, item.FireArm.Attachments, item.Scale) { }

    public SerializedItem(uint id, float durability, uint weaponAttachment, Vector3 scale)
    {
        ID = id;
        Durability = durability;
        WeaponAttachments = weaponAttachment;
        XSize = scale.x;
        YSize = scale.y;
        ZSize = scale.z;
    }

    public uint ID { get; set; }
        
    public float Durability { get; set; }
    public uint WeaponAttachments { get; set; }
    public float XSize { get; set; } = 1f;
    public float YSize { get; set; } = 1f;
    public float ZSize { get; set; } = 1f;

    public SynapseItem Parse() => new(ID)
    {
        Durability = Durability,
        FireArm =
        {
            Attachments = WeaponAttachments
        }
    };

    public static explicit operator SynapseItem(SerializedItem item) => item.Parse();
    public static implicit operator SerializedItem(SynapseItem item) => new (item);
}

[Serializable]
public class SerializedPlayerItem : SerializedItem
{
    public SerializedPlayerItem() { }

    public SerializedPlayerItem(SynapseItem item, short chance, bool provide) 
        : this(item.Id, item.Durability, item.FireArm.Attachments, item.Scale, chance, provide) { }

    public SerializedPlayerItem(uint id, float durability, uint weaponAttachment, Vector3 scale, short chance, bool provide) 
        : base(id, durability, weaponAttachment, scale)
    {
        Chance = chance;
        ProvideFully = provide;
    }

    public short Chance { get; set; } = 100;
    public bool ProvideFully { get; set; } = true;

    public SynapseItem Apply(SynapsePlayer player)
    {
        var item = Parse();

        if (Random.Range(1f, 100f) <= Chance)
            item.EquipItem(player, true, ProvideFully);

        return item;
    }
}

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

[Serializable]
public class SerializedPlayerInventory
{
    public SerializedPlayerInventory() { }
    
    public SerializedPlayerInventory(SynapsePlayer player)
    {
        Ammo.Ammo5 = player.Inventory.AmmoBox[AmmoType.Ammo556X45];
        Ammo.Ammo7 = player.Inventory.AmmoBox[AmmoType.Ammo762X39];
        Ammo.Ammo9 = player.Inventory.AmmoBox[AmmoType.Ammo9X19];
        Ammo.Ammo12 = player.Inventory.AmmoBox[AmmoType.Ammo12Gauge];
        Ammo.Ammo44 = player.Inventory.AmmoBox[AmmoType.Ammo44Cal];

        foreach (var item in player.Inventory.Items)
        {
            Items.Add(new SerializedPlayerItem(item, 100, false));
        }
    }

    public List<SerializedPlayerItem> Items { get; set; } = new();

    public SerializedAmmo Ammo { get; set; } = new();

    public void Apply(SynapsePlayer player)
    {
        player.Inventory.ClearEverything();

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

    public Vector3 Parse() => new(X, Y, Z);

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public static implicit operator Vector3(SerializedVector3 vector) => vector?.Parse() ?? Vector3.zero;
    public static implicit operator SerializedVector3(Vector3 vector) => new(vector);
    public static implicit operator SerializedVector3(Quaternion rotation) => new(rotation.eulerAngles);
    public static implicit operator Quaternion(SerializedVector3 vector) => Quaternion.Euler(vector);
}   

[Serializable]
public class SerializedVector2
{
    public SerializedVector2(Vector2 vector)
    {
        X = vector.x;
        Y = vector.y;
    }

    public SerializedVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public SerializedVector2() { }

    public Vector2 Parse() => new(X, Y);

    public float X { get; set; }
    public float Y { get; set; }

    public static implicit operator Vector2(SerializedVector2 vector) => vector?.Parse() ?? Vector2.zero;
    public static implicit operator SerializedVector2(Vector2 vector) => new (vector);
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

    public Color Parse() => new(R, G, B, A);

    public static implicit operator Color(SerializedColor color) => color.Parse();
    public static implicit operator SerializedColor(Color color) => new(color);
    public static implicit operator Color32(SerializedColor color) => color.Parse();
    public static implicit operator SerializedColor(Color32 color) => new(color);

}

[Serializable]
public class SerializedEffect
{
    public SerializedEffect() { }

    public SerializedEffect(PlayerEffect effect)
    {
        Intensity = effect.Intensity;
        Duration = effect.Duration;
        if (Enum.TryParse(effect.ToString().Split(' ')[0], true, out Effect effectType))
        {
            Effect = effectType;
        }
    }

    public SerializedEffect(Effect effect, byte intensity, float duration)
    {
        Effect = effect;
        Intensity = intensity;
        Duration = duration;
    }

    public Effect Effect { get; set; } = Effect.Asphyxiated;

    public byte Intensity { get; set; } = 1;

    public float Duration { get; set; } = -1;

    public void Apply(SynapsePlayer player) => player.GiveEffect(Effect, Intensity, Duration);

    public static implicit operator SerializedEffect(PlayerEffect effect) => new(effect);
}

[Serializable]
public class SerializedPlayerState
 {
        public SerializedPlayerState() { }

        public SerializedPlayerState(SynapsePlayer player)
        {
            Position = player.RoomPoint;
            Scale = player.Scale;
            RoleType = player.RoleType;
            RoleID = player.RoleID;
            UnitId = player.UnitId;
            UnitName = player.Unit;
            Health = player.Health;
            MaxHealth = player.MaxHealth;
            ArtificialHealth = player.ArtificialHealth;
            MaxArtificialHealth = player.MaxArtificialHealth;
            Stamina = player.Stamina;
            GodMode = player.GodMode;
            NoClip = player.NoClip;
            Bypass = player.Bypass;
            OverWatch = player.OverWatch;
            Invisible = player.Invisible;

            Inventory = new SerializedPlayerInventory(player);

            foreach (var effect in player.PlayerEffectsController._allEffects)
            {
                if (!effect.IsEnabled)
                    continue;

                Effects.Add(effect);
            }
        }

        public RoomPoint Position { get; set; } = new ();

        public SerializedVector3 Scale { get; set; } = Vector3.one;

        public SerializedPlayerInventory Inventory { get; set; } = new();

        public List<SerializedEffect> Effects { get; set; } = new();

        public RoleType RoleType { get; set; }

        public uint RoleID { get; set; }

        public byte UnitId { get; set; } = 0;

        public string UnitName { get; set; } = "";

        public float Health { get; set; } = 100f;

        public float MaxHealth { get; set; } = 100f;

        public float ArtificialHealth { get; set; }

        public float MaxArtificialHealth { get; set; } = 75;

        public float Stamina { get; set; } = 100f;

        public bool GodMode { get; set; }

        public bool NoClip { get; set; }

        public bool Bypass { get; set; }

        public bool OverWatch { get; set; }

        public InvisibleMode Invisible { get; set; }

        public void Apply(SynapsePlayer player, bool applyModes = false)
        {
            if (applyModes)
            {
                player.GodMode = GodMode;
                player.NoClip = NoClip;
                player.Bypass = Bypass;
                player.OverWatch = OverWatch;
                player.Invisible = Invisible;
            }

            player.UnitId = UnitId;
            player.Unit = UnitName;
            player.ChangeRoleLite(RoleType);
            if (RoleID > RoleService.HighestRole)
            {
                player.SpawnCustomRole(RoleID, true);
            }
            else
            {
                player.RemoveCustomRole(DeSpawnReason.Lite);
            }

            player.RoomPoint = Position;

            player.Health = Health;
            player.MaxHealth = MaxHealth;
            player.ArtificialHealth = ArtificialHealth;
            player.MaxArtificialHealth = MaxArtificialHealth;
            player.Stamina = Stamina;
            player.Scale = Scale;

            Inventory.Apply(player);

            foreach (var effect in player.PlayerEffectsController._allEffects)
            {
                effect.OnClassChanged(RoleType.None, RoleType);
            }
            foreach (var effect in Effects)
                effect.Apply(player);
        }

        public static implicit operator SerializedPlayerState(SynapsePlayer player) => new (player);
 }