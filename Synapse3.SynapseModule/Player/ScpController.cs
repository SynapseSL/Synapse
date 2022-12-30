using Synapse3.SynapseModule.Config;
using System.Collections.Generic;

namespace Synapse3.SynapseModule.Player;

public class ScpController
{
    private readonly SynapsePlayer _player;
    private readonly SynapseConfigService _config;

    internal ScpController(SynapsePlayer player, SynapseConfigService config)
    {
        _player = player;
        Scp079 = new(player);
        Scp096 = new Scp096Controller(player);
        Scp106 = new Scp106Controller(player);
        Scp173 = new Scp173Controller(player);
        Scp939 = new Scp939Controller(player);
        _config = config;
    }

    public readonly Scp106Controller Scp106;

    public readonly Scp079Controller Scp079;

    public readonly Scp096Controller Scp096;

    public readonly Scp173Controller Scp173;

    public readonly Scp939Controller Scp939;

    public bool CanTalk => _config.GamePlayConfiguration.SpeakingScp.Contains(_player.RoleID);

    private bool _proximityChat;
    public bool ProximityChat 
    {
        get => CanTalk && _proximityChat;
        set => _proximityChat = value;
    } 
}