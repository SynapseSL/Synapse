namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    //Synapse will not implement a default Database since it would be to heavy and takes the chance away to implement a own Database.
    //So we implement these two methods for plugins that another Module can implement
    public string GetData(string key) => _dataBase.GetPlayerData(this, key);

    public void SetData(string key, string value) => _dataBase.SetPlayerData(this, key, value);
}