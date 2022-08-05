using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    //Synapse will not implement a default Database since it would be to heavy and takes the chance away to implement a own Database.
    //So we implement these two methods for plugins that another Module can implement
    public string GetData(string key)
    {
        var ev = new GetPlayerDataEvent(this, key);
        _serverEvents.GetPlayerData.Raise(ev);
        return ev.Data;
    }

    public void SetData(string key, string value)
    {
        var ev = new SetPlayerDataEvent(this, key, value);
        _serverEvents.SetPlayerData.Raise(ev);
    }
}