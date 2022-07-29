using PlayableScps;

namespace Synapse3.SynapseModule.Player;

public class Scp173Controller
{
    private SynapsePlayer _player;
    
    public Scp173Controller(SynapsePlayer player)
    {
        _player = player;
    }
    
    public Scp173 Scp173 => _player.Hub.scpsController.CurrentScp as Scp173;
    public bool Is096Instance => Scp173 != null;

    public bool IsObserved => Scp173?._isObserved ?? false;

    public float BlinkCooldown
    {
        get => Scp173?._blinkCooldownRemaining ?? 0f;
        set => Scp173._blinkCooldownRemaining = value;
    }
}