using EmbedIO;

namespace Synapse.Network.nodes
{
    public class SynapseNetworkNode : INetworkNode
    {
        public void RegisterWebserverWith(WebServer server)
        {
            server.WithWebApi("/synapse", x => x.RegisterController<SynapseSynapseRouteController>());
            server.WithWebApi("/networksync", x => x.RegisterController<SynapseNetworkSyncController>());
        }

        public void Reconfigure(InstanceAuthority authority)
        {
            Server.Get.NetworkManager.Client.SendMessage(InstanceMessage.CreateBroadcast("Echo", "Hello!"));
        }

        public void StartClient(SynapseNetworkClient client)
        {
        }

        public void ReceiveInstanceMessage(InstanceMessage message)
        {
            if (message.Subject == "Echo") Server.Get.Logger.Info($"{message.Value()} from {message.Sender}");
        }

        public void Hearthbeat()
        {
        }
    }
}