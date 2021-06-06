using Synapse.Api.Items;

namespace Synapse.Api.Exceptions
{
    public class SynapseItemAlreadyRegisteredException : SynapseException
    {
        public SynapseItemAlreadyRegisteredException(string message, CustomItemInformation info) : base(message) => CustomItemInformation = info;

        public CustomItemInformation CustomItemInformation { get; }
    }
}
