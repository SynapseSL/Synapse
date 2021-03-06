using EmbedIO;

namespace Synapse.Network
{
    public interface INetworkNode
    {
        public void RegisterWebserverWith(WebServer server);
        public void Reconfigure(InstanceAuthority authority);
        public void StartClient(SynapseNetworkClient client);
        public void ReceiveInstanceMessage(InstanceMessage message);

        public void Hearthbeat();
    }
}