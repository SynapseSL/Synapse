using System;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseOldGrenade : NetworkSynapseObject
{
    public const string OldGrenadeGuid = "8063e113-c1f1-1514-7bc5-840ea8ee5f01";
    public const string OldFlashBangGuid = "c69da0e5-a829-6a04-c8d9-f404a1073cfe";
    
    public override GameObject GameObject { get; }
    public override ObjectType Type => ObjectType.OldGrenade;
    public override NetworkIdentity NetworkIdentity { get; }

    public SynapseOldGrenade(Vector3 position, Quaternion rotation, bool flash = false)
    {
        var guid = flash ? OldFlashBangGuid : OldGrenadeGuid;
        var prefab = NetworkClient.prefabs[Guid.Parse(guid)];
        GameObject = Object.Instantiate(prefab, position, rotation);
        NetworkIdentity = GameObject.GetComponent<NetworkIdentity>();
        NetworkServer.Spawn(GameObject);
        SetUp();
    }

    internal SynapseOldGrenade(SchematicConfiguration.OldGrenadeConfiguration config, SynapseSchematic schematic) :
        this(config.Position, config.Rotation, config.IsFlash)
    {
        Parent = schematic;
        schematic._oldGrenades.Add(this);
        Scale = config.Scale;

        OriginalScale = config.Scale;
        CustomAttributes = config.CustomAttributes;
        UpdateEveryFrame = config.UpdateEveryFrame;
    }

    private void SetUp()
    {
        Map._synapseOldGrenades.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }

    public override void OnDestroy()
    {
        Map._synapseOldGrenades.Remove(this);
        base.OnDestroy();
        
        
        if (Parent is SynapseSchematic schematic) schematic._oldGrenades.Remove(this);
    }
}