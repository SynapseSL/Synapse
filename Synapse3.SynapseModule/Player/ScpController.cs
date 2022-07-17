using System.Collections.Generic;

namespace Synapse3.SynapseModule.Player;

public class ScpController
{
    private readonly SynapsePlayer _player;
    
    internal ScpController(SynapsePlayer player)
    {
        _player = player;
        Scp079 = new(player);
        Scp096 = new Scp096Controller(player);
    }

    public readonly Scp079Controller Scp079;

    public readonly Scp096Controller Scp096;
}