using Synapse.Api.Roles;
namespace Synapse.Api.Exceptions
{
    public class SynapseRoleAlreadyRegisteredException : SynapseException
    {
        public SynapseRoleAlreadyRegisteredException(string message, RoleInformation info) : base(message) => RoleInformation = info;

        public RoleInformation RoleInformation { get; }
    }
}