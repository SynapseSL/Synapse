using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Player;

public class SynapseServerPlayer : SynapsePlayer
{
    private readonly ServerEvents _serverEvents;
    
    public SynapseServerPlayer()
    {
        _serverEvents = Synapse.Get<ServerEvents>();
    }
    /// <inheritdoc cref="SynapsePlayer.IsServer"/>
    public override PlayerType PlayerType => PlayerType.Server;

    public override void Awake()
    {
        Synapse.Get<PlayerService>().Host = this;
    }

    //Don't Remove this it's a little bit more optimised this way
    public override void OnDestroy() { }

    private void OnApplicationQuit()
    {
        _serverEvents.StopServer.Raise(new StopServerEvent());
    }

    public override TTranslation GetTranslation<TTranslation>(TTranslation translation) => translation.Get();
}