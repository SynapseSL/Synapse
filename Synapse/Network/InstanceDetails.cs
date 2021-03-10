using System;

namespace Synapse.Network
{
    [Serializable]
    public class InstanceDetails
    {
        public string Endpoint { get; set; }
        public string ClientName { get; set; }
        public string ClientUid { get; set; }
        public int Port { get; set; }
    }

    [Serializable]
    public class InstanceDetailsTransmission : SuccessfulStatus
    {
        public InstanceDetails Details { get; set; }
    }
}