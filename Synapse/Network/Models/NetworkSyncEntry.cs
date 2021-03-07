using System;
using Synapse.Network.Models;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkSyncEntry : SerializableObjectWrapper, IStatus

    {
        public string Key { get; set; }
        public string Message { get; set; } = "Ok";
        public bool Successful { get; set; } = true;
    }
}