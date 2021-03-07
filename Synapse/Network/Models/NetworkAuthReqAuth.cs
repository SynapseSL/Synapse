using System;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkAuthReqAuth
    {
        public string ClientIdentifier { get; set; }
        public string Secret { get; set; }
    }
}