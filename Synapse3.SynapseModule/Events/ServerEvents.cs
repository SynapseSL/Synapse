using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Events;

public class ServerEvents: Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<ReloadEvent> Reload = new();
    public readonly EventReactor<PreAuthenticationEvent> PreAuthentication = new();
    public readonly EventReactor<GetPlayerDataEvent> GetPlayerData = new();
    public readonly EventReactor<SetPlayerDataEvent> SetPlayerData = new();
    public readonly EventReactor<GetLeaderBoardEvent> GetLeaderBoard = new();
    public readonly EventReactor<GetDataEvent> GetData = new();
    public readonly EventReactor<SetDataEvent> SetData = new();

    public ServerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Reload);
        _eventManager.RegisterEvent(PreAuthentication);
        _eventManager.RegisterEvent(GetPlayerData);
        _eventManager.RegisterEvent(SetPlayerData);
        _eventManager.RegisterEvent(GetLeaderBoard);
        _eventManager.RegisterEvent(GetData);
        _eventManager.RegisterEvent(SetData);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Reload);
        _eventManager.UnregisterEvent(PreAuthentication);
        _eventManager.UnregisterEvent(GetPlayerData);
        _eventManager.UnregisterEvent(SetPlayerData);
        _eventManager.UnregisterEvent(GetLeaderBoard);
        _eventManager.UnregisterEvent(GetData);
        _eventManager.UnregisterEvent(SetData);
    }
}

public class ReloadEvent : IEvent { }

public class PreAuthenticationEvent : IEvent
{
    private readonly ConnectionRequest _request;

    public PreAuthenticationEvent(ConnectionRequest request)
    {
        _request = request;
    }
    
    public string UserId { get; set; }

    public bool Allow { get; set; } = true;

    public bool Rejected { get; private set; }

    public void Reject(string reason)
    {
        var data = new NetDataWriter();
        data.Put((byte)10);
        data.Put(reason);
        _request.RejectForce(data);
        Rejected = true;
    }
}

public class GetPlayerDataEvent : PlayerEvent
{
    public GetPlayerDataEvent(SynapsePlayer player, string key) : base(player)
    {
        Key = key;
    }

    public string Key { get; }

    public string Data { get; set; } = "";
}

public class SetPlayerDataEvent : PlayerEvent
{
    public SetPlayerDataEvent(SynapsePlayer player, string key, string value) : base(player)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    
    public string Value { get; }
}

public class GetLeaderBoardEvent : IEvent
{
    public GetLeaderBoardEvent(string key, bool orderByHighest)
    {
        Key = key;
        OrderByHighest = orderByHighest;
    }

    public string Key { get; }
    
    public bool OrderByHighest { get; }

    public Dictionary<SynapsePlayer, string> Data { get; set; } = new();
}

public class GetDataEvent : IEvent
{
    public GetDataEvent(string key)
    {
        Key = key;
    }
    
    public string Key { get; }

    public string Data { get; set; } = "";
}

public class SetDataEvent : IEvent
{
    public SetDataEvent(string key, string value)
    {
        Key = key;
        Value = value;
    }
    
    public string Key { get; }
    
    public string Value { get; }
}