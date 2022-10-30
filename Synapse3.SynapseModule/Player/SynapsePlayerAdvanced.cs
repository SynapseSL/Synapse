using System;
using System.Collections.Generic;
using Mirror;
using Neuron.Modules.Configs.Localization;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    internal SetClassEvent setClassStored;
    
    public FakeRoleManager FakeRoleManager { get; }
    
    public SerializedPlayerState State
    {
        get => this;
        set => value.Apply(this, true);
    }

    public void SendFakeSyncVar<TNetworkBehaviour, TValue>(TNetworkBehaviour behaviour, ulong id,
        TValue value) where TNetworkBehaviour : NetworkBehaviour =>
        SendNetworkMessage(_mirror.GetCustomVarMessage(behaviour, id, value));

    public void SendFakeEffectIntensity(Effect effect, byte intensity = 1)
        => SendNetworkMessage(_mirror.GetCustomVarMessage(PlayerEffectsController, writer =>
        {
            writer.WriteUInt64(1); //Which SyncObject will be updated

            //SyncList Specific
            writer.WriteUInt32(1); //The amount of changes
            writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
            writer.WriteUInt32((uint)effect); //effect id/index
            writer.WriteByte(intensity); // Intensity
        }, false));

    public void SendFakeEffectIntensityFor(SynapsePlayer player, Effect effect, byte intensity = 1)
    => SendNetworkMessage(_mirror.GetCustomVarMessage(player.PlayerEffectsController, writer =>
    {
        writer.WriteUInt64(1); //Which SyncObject will be updated

        //SyncList Specific
        writer.WriteUInt32(1); //The amount of changes
        writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
        writer.WriteUInt32((uint)effect); //effect id/index
        writer.WriteByte(intensity); // Intensity
    }, false));

    public void AttachSynapseObject(ISynapseObject so, Vector3 offset)
    {
        so.Rotation = transform.rotation;
        Transform transform1;
        so.Position = (transform1 = transform).TransformPoint(offset);
        so.GameObject.transform.parent = transform1;
    }

    public ItemInventory Inventory { get; }

    public BroadcastList ActiveBroadcasts { get; }
    
    public ScpController ScpController { get; }

    public Dictionary<string, object> Data { get; set; } = new();

    public void SendNetworkMessage<TNetworkMessage>(TNetworkMessage msg, int channel = 0)
        where TNetworkMessage : struct, NetworkMessage =>
        Connection?.Send(msg, channel);

    public virtual TTranslation GetTranslation<TTranslation>(TTranslation translation) where TTranslation : Translations<TTranslation>, new()
    {
        var language = new List<string>();
        
        var playerTranslation = GetData("language");
        if (!string.IsNullOrWhiteSpace(playerTranslation))
            language.Add(playerTranslation);
        
        language.AddRange(Synapse.Get<SynapseConfigService>().HostingConfiguration.Language);
        
        return translation.WithLocale(language.ToArray());
    }

    private float _walkSpeed;
    private bool _customWalkSpeed;
    public float WalkSpeed
    {
        get => _customWalkSpeed ? _walkSpeed : ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier;
        set
        {
            if (Math.Abs(value - ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier) < 0.1f)
            {
                _customWalkSpeed = false;
            }
            else
            {
                _customWalkSpeed = true;
                _walkSpeed = value;   
            }

            SendNetworkMessage(_mirror.GetCustomVarMessage(ServerConfigSynchronizer.Singleton, 2ul, value));
        }
    }
    
    private float _sprintSpeed;
    private bool _customSprintSpeed;
    public float SprintSpeed
    {
        get => _customSprintSpeed ? _sprintSpeed : ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier;
        set
        {
            if (Math.Abs(value - ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier) < 0.1f)
            {
                _customSprintSpeed = false;
            }
            else
            {
                _customSprintSpeed = true;
                _sprintSpeed = value;
            }
            
            SendNetworkMessage(_mirror.GetCustomVarMessage(ServerConfigSynchronizer.Singleton, 4ul, value));
        }
    }
}