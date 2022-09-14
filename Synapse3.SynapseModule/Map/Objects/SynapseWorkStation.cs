using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseWorkStation : NetworkSynapseObject, IJoinUpdate
{
    private readonly MirrorService _mirror;
    private readonly PlayerService _player;
    
    public static WorkstationController Prefab { get; internal set; }
    
    public WorkstationController WorkstationController { get; }
    public override GameObject GameObject => WorkstationController.gameObject;
    public override ObjectType Type => ObjectType.Workstation;
    public override NetworkIdentity NetworkIdentity => WorkstationController.netIdentity;
    public override void OnDestroy()
    {
        Map._synapseWorkStations.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._workStations.Remove(this);
    }
    
    public string Name => GameObject.name;
    
    public SynapsePlayer KnownUser
    {
        get => WorkstationController._knownUser.GetSynapsePlayer();
        set => WorkstationController._knownUser = value.Hub;
    }

    public WorkstationState State
    {
        get => (WorkstationState)WorkstationController.Status;
        set => WorkstationController.NetworkStatus = (byte)value;
    }

    private SynapseWorkStation()
    {
        _mirror = Synapse.Get<MirrorService>();
        _player = Synapse.Get<PlayerService>();
    }
    public SynapseWorkStation(Vector3 position, Quaternion rotation, Vector3 scale) : this()
    {
        WorkstationController = CreateNetworkObject(Prefab, position, rotation, scale);
        NeedsJoinUpdate = true;
        SetUp();
    }
    internal SynapseWorkStation(WorkstationController station) : this()
    {
        WorkstationController = station;
        SetUp();
    }

    internal SynapseWorkStation(SchematicConfiguration.SimpleUpdateConfig configuration,
        SynapseSchematic schematic) :
        this(configuration.Position, configuration.Rotation, configuration.Scale)
    {
        Parent = schematic;
        schematic._workStations.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        Update = configuration.Update;
        UpdateFrequency = configuration.UpdateFrequency;
    }
    private void SetUp()
    {
        Map._synapseWorkStations.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        _player.JoinUpdates.Add(this);
    }

    public bool NeedsJoinUpdate { get; }

    public void UpdatePlayer(SynapsePlayer player)
    {
        player.SendNetworkMessage(_mirror.GetSpawnMessage(NetworkIdentity));
    }
}