using System;
using System.Collections.Generic;

namespace Synapse.Network.Models
{
    [Serializable]
    public class PingResponse : SuccessfulStatus
    {
        public bool Authenticated { get; set; }
        public List<InstanceMessage> Messages { get; set; }
        public string LatestVarHash { get; set; }
        public List<string> ConnectedClients { get; set; }
    }
}