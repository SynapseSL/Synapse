namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    //Synapse will not implement a default Database since it would be to heavy and takes the chance away to implement a own Database.
    //So we implement these methods for plugins that another Module can implement
    public string GetData(string key) => _database.GetPlayerData(this, key);

    public void SetData(string key, string value) => _database.SetPlayerData(this, key, value);

    public void DeleteData(string key) => _database.DeletePlayerData(this, key);
}