using MEC;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles.Voice;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map;

public class IntercomService : Service
{
    private RoundEvents _round;
    private PlayerService _player;
    public Intercom Intercom => Intercom._singleton;

    public IntercomService(RoundEvents round, PlayerService player)
    {
        _round = round;
        _player = player;
    }

    public override void Enable()
    {
    }

    public override void Disable()
    {
    }

    public SynapsePlayer Speaker
    {
        get
        {
            if (Intercom._curSpeaker == null) return null;

            return Intercom._curSpeaker.GetSynapsePlayer();
        }
        set { }
        //TODO:
        //Intercom.RequestTransmission(value.gameObject);
    }

    /*
    public string DisplayText
    {
        get => Intercom.CustomContent;
        set => Intercom.CustomContent = string.IsNullOrWhiteSpace(value) ? null : value;
    }
    */

    public float RemainingTime
    {
        get => Intercom.RemainingTime;
        set => Intercom._nextTime = value + NetworkTime.time;
    }
}