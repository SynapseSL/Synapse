using LightContainmentZoneDecontamination;
using LiteNetLib;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using static RoundSummary;

namespace Synapse3.SynapseModule.Events;

public partial class PlayerEvents
{

}

public partial class RoundEvents
{
    private bool _firstTime = true;
    [PluginEvent(ServerEventType.WaitingForPlayers)]
    public void RoundWaitingHook()
    {
        Waiting.RaiseSafely(new RoundWaitingEvent(_firstTime));

        _firstTime = false;
    }

    [PluginEvent(ServerEventType.RoundStart)]
    public void RoundStartHook() => Start.RaiseSafely(new RoundStartEvent());

    [PluginEvent(ServerEventType.RoundEnd)]
    public void RoundEndHook(LeadingTeam leadingTeam) => End.RaiseSafely(new RoundEndEvent(leadingTeam));
    
    [PluginEvent(ServerEventType.RoundRestart)]
    public void RoundRestartHook() => Restart.RaiseSafely(new RoundRestartEvent());

    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    public bool DecontaminationHook()
    {
        var ev = new DecontaminationEvent();
        Decontamination.RaiseSafely(new DecontaminationEvent());
        if (!ev.Allow)
        {
            DecontaminationController.Singleton.NetworkDecontaminationOverride =
                DecontaminationController.DecontaminationStatus.None;
        }
        return ev.Allow;
    }
}

public partial class ServerEvents
{
    [PluginEvent(ServerEventType.PlayerPreauth)]
    public PreauthCancellationData PreAuth(string userId, string address, long number,
        CentralAuthPreauthFlags centralFlags, string countryCode, byte[] array, ConnectionRequest request, int position)
    {
        var ev = new PreAuthenticationEvent(userId, address, countryCode, centralFlags);
        PreAuthentication.RaiseSafely(ev);
        return ev.ReturningData;
    }
}