using Synapse.Config;
using System.ComponentModel;

namespace Synapse.Permission
{
    public class ServerSection : IConfigSection
    {
        [Description("If Enabled the GlobalBanTeam gets their global Permissions")]
        public bool GlobalBanTeamAccess = true;

        [Description("If Enabled the Manager gets their global Permissions")]
        public bool ManagerAccess = true;

        [Description("If Enabled the Staff gets their global Permissions")]
        public bool StaffAccess = true;
    }
}