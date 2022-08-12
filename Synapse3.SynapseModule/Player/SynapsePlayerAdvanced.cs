using System.Collections.Generic;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Modules.Configs.Localization;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    internal SetClassEvent setClassStored;
    
    public SerializedPlayerState State
    {
        get => this;
        set => value.Apply(this, true);
    }

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

    public void SendNetworkMessage<TNetworkMessage>(TNetworkMessage msg, int channel = 0)
        where TNetworkMessage : struct, NetworkMessage =>
        Connection?.Send(msg, channel);

    public TTranslation GetTranslation<TTranslation>(TTranslation translation) where TTranslation : Translations<TTranslation>, new()
    {
        var language = new List<string>();
        
        var playerTranslation = GetData("language");
        if (!string.IsNullOrWhiteSpace(playerTranslation))
            language.Add(playerTranslation);
        
        language.AddRange(Synapse.Get<SynapseConfigService>().HostingConfiguration.Language);
        
        //TODO: NuGet Update
        return translation.WithLocale(language[0]);
    }
}