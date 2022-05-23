namespace Synapse.Api.Exceptions
{
    public class SynapseItemNotFoundException : SynapseException
    {
        public SynapseItemNotFoundException(string message, int id) : base(message) => ID = id;

        public int ID { get; }
    }
}