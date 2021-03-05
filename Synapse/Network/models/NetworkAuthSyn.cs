using System;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkAuthSyn
    {
        public string ClientName { get; set; }
        public string PublicKey { get; set; }
    }
}