using System;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkReqAuth
    {
        public string ClientIdentifier { get; set; }
        public string Secret { get; set; }
    }
}