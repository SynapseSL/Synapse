using System;
using System.Collections.Generic;
using Synapse.Network.Models;

namespace Synapse.Network
{
    [Serializable]
    public class InstanceDetails
    {
        public string Endpoint { get; set; }
        public string ClientName { get; set; }
        public string ClientUid { get; set; }
        public int Port { get; set; }
        public List<KeyValueObjectWrapper> SyncEntries { get; set; }
    }

    [Serializable]
    public class InstanceDetailsTransmission : SuccessfulStatus
    {
        public InstanceDetails Details { get; set; }
    }

    [Serializable]
    public class InstanceDetailsListTransmission : SuccessfulStatus
    {
        public InstanceDetails[] Details { get; set; }
    }
}