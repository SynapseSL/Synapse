using EmbedIO;

namespace Synapse.Network.nodes
{
    public class SynapseNetworkNode : INetworkNode
    {
        public void RegisterWebserverWith(WebServer server)
        {
            server.WithWebApi("/synapse", x =>
                x.RegisterController<SynapseAuthController>());
        }

        public void Reconfigure(InstanceAuthority authority)
        {
            
        }

        public void StartClient(SynapseNetworkClient client)
        {
            
        }
    }
}