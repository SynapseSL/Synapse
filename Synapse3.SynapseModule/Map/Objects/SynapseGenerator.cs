using MapGeneration.Distributors;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseGenerator : StructureSyncSynapseObject
{
    public static Scp079Generator GeneratorPrefab { get; internal set; }
    
    
    public Scp079Generator Generator { get; }
    public override GameObject GameObject => Generator.gameObject;
    public override NetworkIdentity NetworkIdentity => Generator.netIdentity;
    public override ObjectType Type => ObjectType.Generator;
    public override void OnDestroy()
    {
        Map._synapseGenerators.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._generators.Remove(this);
    }
    public string Name => GameObject.name;

    public bool Open
    {
        get => Generator.HasFlag(Generator._flags, Scp079Generator.GeneratorFlags.Open);
        set
        {
            Generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, value);
            Generator._targetCooldown = Generator._doorToggleCooldownTime;
        }
    }

    public bool Locked
    {
        get => !Generator.HasFlag(Generator._flags, Scp079Generator.GeneratorFlags.Unlocked);
        set
        {
            Generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, !value);
            Generator._targetCooldown = Generator._unlockCooldownTime;
        }
    }

    public bool Active
    {
        get => Generator.Activating;
        set
        {
            Generator.Activating = value;
            
            if(value)
                Generator._leverStopwatch.Reset();

            Generator._targetCooldown = Generator._doorToggleCooldownTime;
        }
    }

    public bool Engaged
    {
        get => Generator.Engaged;
        set => Generator.Engaged = value;
    }

    public short Time
    {
        get => Generator._syncTime;
        set => Generator.Network_syncTime = value;
    }

    public SynapseGenerator(Vector3 pos, Quaternion rotation, Vector3 scale)
    {
        Generator = CreateNetworkObject(GeneratorPrefab, pos, rotation, scale);
        SetUp();
    }

    internal SynapseGenerator(Scp079Generator generator)
    {
        Generator = generator;
        SetUp();
    }
    internal SynapseGenerator(SchematicConfiguration.SimpleUpdateConfig configuration,
        SynapseSchematic schematic) :
        this(configuration.Position,configuration.Rotation,configuration.Scale)
    {
        Parent = schematic;
        schematic._generators.Add(this);
        GameObject.transform.parent = schematic.GameObject.transform;
        
        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        UpdateEveryFrame = configuration.UpdateEveryFrame;
    }
    private void SetUp()
    {
        Map._synapseGenerators.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }
}