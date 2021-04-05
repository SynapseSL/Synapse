namespace Synapse.Network.Models
{
    public class NetBroadcast
    {
        public NetworkPlayer Player { get; set; }
        public string Message { get; set; }
        public ushort Duration { get; set; } = 5;
    }
}