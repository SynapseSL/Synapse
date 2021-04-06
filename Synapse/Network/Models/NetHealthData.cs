using System;

namespace Synapse.Network.Models
{
    public class NetHealthData
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public long StartupTimestamp { get; set; }
    }
}