using MEC;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map;

public class IntercomService : Service
{
    private RoundEvents _round;
    private PlayerService _player;
    public Intercom Intercom { get; private set; }

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

    public SynapsePlayer Speaker
    {
        get
        {
            if (Intercom.Networkspeaker == null) return null;

            return Intercom.Networkspeaker.GetPlayer();
        }
        set => Intercom.RequestTransmission(value.gameObject);
    }

    public string DisplayText
    {
        get => Intercom.CustomContent;
        set => Intercom.CustomContent = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public ushort RemainingTime
    {
        get => Intercom.NetworkIntercomTime;
        set => Intercom.NetworkIntercomTime = value;
    }

    private void GetIntercom(RoundWaitingEvent ev)
    {
        Intercom = _player.Host.GetComponent<Intercom>();
    }
}