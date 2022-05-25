namespace Synapse.Api.Exceptions
{
    public class SynapseRoleNotFoundException : SynapseException
    {
        public SynapseRoleNotFoundException(string message, int id) : base(message) 
            => ID = id;

        public SynapseRoleNotFoundException(string message, string name) : base(message) 
            => Name = name;

        public SynapseRoleNotFoundException(string message, int id, string name) : base(message)
        {
            ID = id;
            Name = name;
        }

        public int ID { get; }

        public string Name { get; set; }
    }
}
