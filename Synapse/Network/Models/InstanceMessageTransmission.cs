using System;
using System.Collections.Generic;

namespace Synapse.Network
{
    [Serializable]
    public class InstanceMessageTransmission : SuccessfulStatus
    {
        public List<string> Receivers { get; set; }
    }
}