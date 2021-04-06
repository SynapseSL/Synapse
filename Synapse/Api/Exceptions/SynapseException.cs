using System;

namespace Synapse.Api.Exceptions
{
    public class SynapseException : Exception
    {
        public SynapseException() { }

        public SynapseException(string message) : base(message) { }

        public SynapseException(string message, Exception inner) : base(message,inner) { }
    }
}
