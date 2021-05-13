using System;
using System.Collections.Generic;

namespace Synapse.Network.Models
{
    [Serializable]
    public class InstanceMessageTransmission : SuccessfulStatus
    {
        public List<string> Receivers { get; set; }
    }
}