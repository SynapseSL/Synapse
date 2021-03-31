using System;

namespace Synapse.Network.Models
{
    [Serializable]
    public class NetworkSyncEntry : KeyValueObjectWrapper, IStatus

    {
        public string Message { get; set; } = "Ok";
        public bool Successful { get; set; } = true;
    }
}