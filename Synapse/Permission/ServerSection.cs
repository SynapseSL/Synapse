using Synapse.Config;

namespace Synapse.Permission
{
    public class ServerSection : IConfigSection
    {
        public bool UsePassword = false;

        public bool StaffAcces = true;

        public bool ManagerAcces = true;

        public bool BanTeamAcces = true;
    }
}
