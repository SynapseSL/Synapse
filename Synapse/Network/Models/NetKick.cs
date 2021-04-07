namespace Synapse.Network.Models
{
    public class NetKick
    {
        public NetworkPlayer Player { get; set; }
        public string Issuer { get; set; }
        public string Message { get; set; }
        public string Note { get; set; }
    }
}