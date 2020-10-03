using Synapse.Config;

namespace Synapse.Permission
{
    public class ServerSection : IConfigSection
    {
        public bool GlobalBanTeamAccess = true;

        public bool ManagerAccess = true;

        public bool StaffAccess = true;
    }
}
