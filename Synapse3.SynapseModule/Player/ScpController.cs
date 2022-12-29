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
        Scp106 = new Scp106Controller(player);
        Scp173 = new Scp173Controller(player);
        Scp939 = new Scp939Controller(player);
    }

    public readonly Scp106Controller Scp106;

    public readonly Scp079Controller Scp079;

    public readonly Scp096Controller Scp096;

    public readonly Scp173Controller Scp173;

    public readonly Scp939Controller Scp939;


}