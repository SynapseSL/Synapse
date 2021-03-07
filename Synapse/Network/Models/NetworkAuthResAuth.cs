using System;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkAuthResAuth : SuccessfulStatus
    {
        public string SessionToken { get; set; }
    }
}