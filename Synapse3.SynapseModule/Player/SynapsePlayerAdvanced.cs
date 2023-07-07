using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mirror;
using Neuron.Modules.Configs.Localization;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.KeyBind;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public ItemInventory Inventory { get; }

    public BroadcastList ActiveBroadcasts { get; }

    public TextHintList ActiveHint { get; }
    
    public ScpController.MainScpController MainScpController { get; }

    public Dictionary<string, object> Data { get; set; } = new();

    internal Dictionary<KeyCode , List<IKeyBind>> _binds = new();
    public ReadOnlyDictionary<KeyCode, List<IKeyBind>> Binds => new(_binds);

    public SerializedPlayerState State
    {
        get => this;
        set => value.Apply(this, true);
    }

    public void SendNetworkMessage<TNetworkMessage>(TNetworkMessage msg, int channel = 0)
        where TNetworkMessage : struct, NetworkMessage =>
        Connection?.Send(msg, channel);

    /// <inheritdoc cref="MirrorService.GetCustomVarMessage{TNetworkBehaviour, TValue}(TNetworkBehaviour, ulong, TValue)"/>
    public void SendFakeSyncVar<TNetworkBehaviour, TValue>(TNetworkBehaviour behaviour, ulong id,
        TValue value) where TNetworkBehaviour : NetworkBehaviour =>
        SendNetworkMessage(_mirror.GetCustomVarMessage(behaviour, id, value));

    public void SendFakeEffectIntensity(Effect effect, byte intensity = 1)
        => SendNetworkMessage(_mirror.GetCustomVarMessage(PlayerEffectsController, writCustomObjectData: writer =>
        {
            writer.WriteULong(1); //Which SyncObject will be updated

            //SyncList Specific
            writer.WriteUInt(1); //The amount of changes
            writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
            writer.WriteUInt((uint)effect); //effect id/index
            writer.WriteByte(intensity); // Intensity
        }));

    public void SendFakeEffectIntensityFor(SynapsePlayer player, Effect effect, byte intensity = 1)
    => SendNetworkMessage(_mirror.GetCustomVarMessage(player.PlayerEffectsController, writCustomObjectData: writer =>
    {
        //TODO: Redo it Mirror change
        writer.WriteULong(1); //Which SyncObject will be updated

        //SyncList Specific
        writer.WriteUInt(1); //The amount of changes
        writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
        writer.WriteUInt((uint)effect); //effect id/index
        writer.WriteByte(intensity); // Intensity
    }));

    public void AttachSynapseObject(ISynapseObject so, Vector3 offset)
    {
        so.Rotation = transform.rotation;
        Transform transform1;
        so.Position = (transform1 = transform).TransformPoint(offset);
        so.GameObject.transform.parent = transform1;
    }

    public virtual TTranslation GetTranslation<TTranslation>(TTranslation translation) where TTranslation : Translations<TTranslation>, new()
    {
        var language = new List<string>();

        var playerTranslation = GetData("language");
        if (!string.IsNullOrWhiteSpace(playerTranslation))
            language.Add(playerTranslation);

        language.AddRange(Synapse.Get<SynapseConfigService>().HostingConfiguration.Language);

        return translation.WithLocale(language.ToArray());
    }

    public float MovementMultiplier
    {
        get => VanillaInventory._syncMovementMultiplier;
        set => VanillaInventory.Network_syncMovementLimiter = value;
    }

    public float MovementLimiter
    {
        get => VanillaInventory._syncMovementLimiter;
        set => VanillaInventory.Network_syncMovementMultiplier = value;
    }
}