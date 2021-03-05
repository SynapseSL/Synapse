using System;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkAuthAck : SuccessfulStatus
    {
        public string PublicKey { get; set; }
        public string ClientIdentifier { get; set; }
        public int MigrationPriority { get; set; }
    }
}