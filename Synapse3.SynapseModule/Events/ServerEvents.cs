using System;
using System.Reflection;
using LiteNetLib;
using LiteNetLib.Utils;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using EventManager = Neuron.Core.Events.EventManager;

namespace Synapse3.SynapseModule.Events;

public partial class ServerEvents : Service
{
    private readonly EventManager _eventManager;
    private readonly Synapse _synapse;

    public readonly EventReactor<ReloadEvent> Reload = new();
    public readonly EventReactor<PreAuthenticationEvent> PreAuthentication = new();
    public readonly EventReactor<StopServerEvent> StopServer = new();

    public ServerEvents(EventManager eventManager, Synapse synapse)
    {
        _eventManager = eventManager;
        _synapse = synapse;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Reload);
        _eventManager.RegisterEvent(PreAuthentication);
        PluginAPI.Events.EventManager.RegisterEvents(_synapse,this);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Reload);
        _eventManager.UnregisterEvent(PreAuthentication);
    }
}

public class ReloadEvent : IEvent { }

public class PreAuthenticationEvent : IEvent
{
    public PreAuthenticationEvent(string userId, string address, string country, CentralAuthPreauthFlags flags)
    {
        UserId = userId;
        Address = address;
        Country = country;
        Flags = flags;
        ReturningData = PreauthCancellationData.Accept();
    }


    public string UserId { get; }
    
    public string Address { get; }
    
    public string Country { get; }
    
    public CentralAuthPreauthFlags Flags { get; }
    
    public PreauthCancellationData ReturningData { get; set; }

    public void Reject(string customReason, bool isForced = false) =>
        ReturningData = PreauthCancellationData.Reject(customReason, isForced);
    
    public void Reject(RejectionReason reason, bool isForced = false) =>
        ReturningData = PreauthCancellationData.Reject(reason, isForced);

    public void Delay(byte seconds, bool isForced = false) =>
        ReturningData = PreauthCancellationData.RejectDelay(seconds, isForced);

    public void Redirect(ushort port, bool isForced = false) =>
        ReturningData = PreauthCancellationData.RejectRedirect(port, isForced);

    public void Ban(string reason, DateTime expiration, bool isForced = false) =>
        ReturningData = PreauthCancellationData.RejectBanned(reason, expiration, isForced);
    
    public void Ban(string reason, long expiration, bool isForced = false) =>
        ReturningData = PreauthCancellationData.RejectBanned(reason, expiration, isForced);
}

public class StopServerEvent : IEvent { }