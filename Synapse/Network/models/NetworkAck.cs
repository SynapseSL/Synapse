using System;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkAck
    {
        public string PublicKey { get; set; }
        public string ClientIdentifier { get; set; }
        public int MigrationPriority { get; set; }
    }
}