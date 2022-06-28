using System;
using System.Collections.Generic;
using Syml;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

[Serializable]
public class SchematicConfiguration : IDocumentSection
{
    /// <summary>
    /// This is to determine if the Schematic should be removed on Reload
    /// </summary>
    [NonSerialized] internal bool Reload = true;
    
    public string Name { get; set; }
    public int ID { get; set; }
    public List<string> CustomAttributes { get; set; }

    public List<PrimitiveConfiguration> Primitives { get; set; } = new();
    public List<LightSourceConfiguration> Lights { get; set; } = new();
    public List<TargetConfiguration> Targets { get; set; } = new();
    public List<ItemConfiguration> Items { get; set; } = new();
    public List<SimpleUpdateConfig> WorkStations { get; set; } = new ();
    public List<DoorConfiguration> Doors { get; set; } = new ();
    public List<CustomObjectConfiguration> CustomObjects { get; set; } = new ();
    public List<RagdollConfiguration> Ragdolls { get; set; } = new ();
    public List<DummyConfiguration> Dummies { get; set; } = new ();
    public List<SimpleUpdateConfig> Generators { get; set; } = new ();
    public List<LockerConfiguration> Lockers { get; set; } = new ();
    public List<OldGrenadeConfiguration> OldGrenades { get; set; } = new ();


    public abstract class DefaultConfig
    {
        public SerializedVector3 Position { get; set; } = Vector3.zero;

        public SerializedVector3 Rotation { get; set; } = Vector3.zero;

        public SerializedVector3 Scale { get; set; } = Vector3.one;

        public List<string> CustomAttributes { get; set; } = new ();
    }

    public class SimpleUpdateConfig : DefaultConfig
    {
        public bool UpdateEveryFrame { get; set; } = false;
    }
    
    public class OldGrenadeConfiguration : SimpleUpdateConfig
    {
        public bool IsFlash { get; set; }
    }

    public class PrimitiveConfiguration : DefaultConfig
    {
        public PrimitiveType PrimitiveType { get; set; }

        public bool Physics { get; set; }

        public SerializedColor Color { get; set; }
    }

    public class LightSourceConfiguration : DefaultConfig
    {
        public SerializedColor Color { get; set; }

        public float LightIntensity { get; set; }

        public float LightRange { get; set; }

        public bool LightShadows { get; set; }
    }

    public class TargetConfiguration : DefaultConfig
    {
        public SynapseTarget.TargetType TargetType { get; set; }
    }

    public class ItemConfiguration : DefaultConfig
    {
        public ItemType ItemType { get; set; }

        public bool CanBePickedUp { get; set; }

        public bool Physics { get; set; }

        public float Durabillity { get; set; }

        public uint Attachments { get; set; }
    }

    public class DoorConfiguration : SimpleUpdateConfig
    {
        public SynapseDoor.SpawnableDoorType DoorType { get; set; }

        public bool Open { get; set; }

        public bool Locked { get; set; }
    }

    public class CustomObjectConfiguration : DefaultConfig
    {
        public int ID { get; set; }
    }

    public class RagdollConfiguration : DefaultConfig
    {
        public string Nick { get; set; }

        public RoleType RoleType { get; set; }

        public DamageType DamageType { get; set; }
    }

    public class DummyConfiguration : DefaultConfig
    {
        public RoleType Role { get; set; }

        public ItemType HeldItem { get; set; }

        public string Name { get; set; }

        public string Badge { get; set; }

        public string BadgeColor { get; set; }
    }

    public class LockerConfiguration : SimpleUpdateConfig
    {
        public SynapseLocker.LockerType LockerType { get; set; }

        public List<LockerChamber> Chambers { get; set; } = new List<LockerChamber>();

        public bool DeleteDefaultItems { get; set; }

        public class LockerChamber
        {
            public List<ItemType> Items { get; set; } = new List<ItemType>();
        }
    }
}