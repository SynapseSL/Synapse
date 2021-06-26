namespace Synapse.Client
{
    public class SynapseServerListMark
    {
        [Swan.Formatters.JsonProperty("onlinePlayers")]
        public int OnlinePlayers { get; set; }

        [Swan.Formatters.JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }

        [Swan.Formatters.JsonProperty("info")]
        public string Info { get; set; }
    }
}
