using System;
using LightContainmentZoneDecontamination;
using LiteNetLib;
using Neuron.Core.Logging;
using PlayerRoles;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Enums;
using PluginAPI.Events;

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
        try
        {
            Waiting.Raise(new RoundWaitingEvent(_firstTime));
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Events: Round Waiting Event failed:\n" + ex);
        }

        _firstTime = false;
    }

    [PluginEvent(ServerEventType.RoundStart)]
    public void RoundStartHook()
    {
        try
        {
            Start.Raise(new RoundStartEvent());
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Events: Round Start Event failed:\n" + ex);
        }
    }
    
    [PluginEvent(ServerEventType.RoundEnd)]
    public void RoundEndHook()
    {
        try
        {
            End.Raise(new RoundEndEvent());
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Events: Round End Event failed:\n" + ex);
        }
    }
    
    [PluginEvent(ServerEventType.RoundRestart)]
    public void RoundRestartHook()
    {
        try
        {
            Restart.Raise(new RoundRestartEvent());
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Events: Round Restart Event failed:\n" + ex);
        }
    }

    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    public bool DecontaminationHook()
    {
        try
        {
            //TODO: Improve this Event
            var ev = new DecontaminationEvent();
            Decontamination.Raise(new DecontaminationEvent());
            if (!ev.Allow)
            {
                DecontaminationController.Singleton.NetworkDecontaminationOverride =
                    DecontaminationController.DecontaminationStatus.None;
            }
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Events: Decontamination Event failed:\n" + ex);
            return true;
        }
    }
}

public partial class ServerEvents
{
    [PluginEvent(ServerEventType.PlayerPreauth)]
    public PreauthCancellationData PreAuth(string userId, string address, long number,
        CentralAuthPreauthFlags centralFlags, string countryCode, byte[] array, ConnectionRequest request, int position)
    {
        try
        {
            var ev = new PreAuthenticationEvent(userId, address, countryCode, centralFlags);
            PreAuthentication.Raise(ev);
            return ev.ReturningData;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Events: Pre Authentication Event failed:\n" + ex);
            return PreauthCancellationData.Accept();
        }
    }
}