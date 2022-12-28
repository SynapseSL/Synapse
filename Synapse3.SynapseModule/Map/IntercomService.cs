using Mirror;
using Neuron.Core.Meta;
using PlayerRoles.Voice;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map;

public class IntercomService : Service
{
    private readonly RoundEvents _round;
    private readonly PlayerService _player;
    public Intercom Intercom { get; private set; }
    public IntercomDisplay Display { get; private set; }

    public IntercomService(RoundEvents round, PlayerService player)
    {
        _round = round;
        _player = player;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(GetIntercom);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(GetIntercom);
    }

    public IntercomState State
    {
        get => Intercom.State;
        set => Intercom.Network_state = (byte)value;
    }

    public SynapsePlayer Speaker
    {
        get => Intercom._curSpeaker == null ? null : Intercom._curSpeaker.GetSynapsePlayer();
        set => Intercom._curSpeaker = value?.Hub;
    }
    
    public string DisplayText
    {
        get => Display.Network_overrideText;
        set => Display.Network_overrideText = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public float RemainingTime
    {
        get => Intercom.RemainingTime;
        set => Intercom._nextTime = value + NetworkTime.time;
    }

    private void GetIntercom(RoundWaitingEvent ev)
    {
        Intercom = Intercom._singleton;
        Display = IntercomDisplay._singleton;
    }
}