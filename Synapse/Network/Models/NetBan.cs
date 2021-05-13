namespace Synapse.Network.Models
{
    public class NetBan
    {
        public NetworkPlayer Player { get; set; }
        public string Message { get; set; }
        public string Note { get; set; }
        public int Duration { get; set; }
    }
}