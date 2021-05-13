using System;

namespace Synapse.Network.Models
{
    [Serializable]
    public class NetworkAuthReqAuth
    {
        public string ClientIdentifier { get; set; }
        public string Secret { get; set; }
    }
}