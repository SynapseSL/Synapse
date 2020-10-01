using Synapse.Config;

namespace Synapse.Permission
{
    public class ServerSection : IConfigSection
    {
        public bool UsePassword = false;

        public bool GlobalAccess = true;
    }
}
