using System;

namespace Synapse.Network.Models
{
    [Serializable]
    public class NetworkAuthResAuth : SuccessfulStatus
    {
        public string SessionToken { get; set; }
    }
}