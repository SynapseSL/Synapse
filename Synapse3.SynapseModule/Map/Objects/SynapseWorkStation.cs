using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseWorkStation : NetworkSynapseObject
{
    public static WorkstationController Prefab { get; internal set; }
    
    public WorkstationController WorkstationController { get; }
    public override GameObject GameObject => WorkstationController.gameObject;
    public override ObjectType Type => ObjectType.Workstation;
    public override NetworkIdentity NetworkIdentity => WorkstationController.netIdentity;
    public override void OnDestroy()
    {
        Map._synapseWorkStations.Remove(this);
        base.OnDestroy();
    }
    
    public string Name => GameObject.name;
    
    public SynapsePlayer KnownUser
    {
        get => WorkstationController._knownUser.GetPlayer();
        set => WorkstationController._knownUser = value.Hub;
    }

    public WorkstationState State
    {
        get => (WorkstationState)WorkstationController.Status;
        set => WorkstationController.NetworkStatus = (byte)value;
    }

    public SynapseWorkStation(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        WorkstationController = CreateNetworkObject(Prefab, position, rotation, scale);
        SetUp();
    }
    internal SynapseWorkStation(WorkstationController station)
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
        GameObject.transform.parent = schematic.GameObject.transform;
        
        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        UpdateEveryFrame = configuration.UpdateEveryFrame;
    }
    private void SetUp()
    {
        Map._synapseWorkStations.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }
}