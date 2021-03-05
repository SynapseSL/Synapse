using EmbedIO;

namespace Synapse.Network
{
    public interface INetworkNode
    {
        void RegisterWebserverWith(WebServer server);
        void Reconfigure(InstanceAuthority authority);
        void StartClient(SynapseNetworkClient client);
    }
}