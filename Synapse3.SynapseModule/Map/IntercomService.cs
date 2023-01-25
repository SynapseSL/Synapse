using Mirror;
using Neuron.Core.Meta;
using PlayerRoles.Voice;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map;

public class IntercomService : Service
{
    public Intercom Intercom => Intercom._singleton;
    public IntercomDisplay Display => IntercomDisplay._singleton;

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
}