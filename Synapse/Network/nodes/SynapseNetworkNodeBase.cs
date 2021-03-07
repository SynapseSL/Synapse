using System;
using EmbedIO;
using Swan;
using Synapse.Network.Routes;

namespace Synapse.Network.nodes
{
    public class SynapseNetworkNodeBase : NetworkNodeBase
    {
        private bool _checked;

        public override void RegisterWebserverWith(WebServer server)
        {
            server.WithWebApi("/synapse", x => x.RegisterController<SynapseSynapseRouteController>());
            server.WithWebApi("/networksync", x => x.RegisterController<SynapseNetworkSyncController>());
        }

        public override void Reconfigure(InstanceAuthority authority)
        {
        }

        public async void CheckPings()
        {
            var client = Server.Get.NetworkManager.Client;
            var clients = await GetClients();
            clients.Remove(client.ClientIdentifier);
            foreach (var uid in clients)
            {
                Server.Get.Logger.Warn($"Pinging {uid}");
                var begin = DateTimeOffset.Now;
                var response = await SendMessageAndAwaitResponse(InstanceMessage.CreateSend("Ping", "", uid));
                Server.Get.Logger.Warn("Response Awaited!");
                var offset = DateTimeOffset.Parse(response.Value<string>());
                var delay = offset.Subtract(begin).TotalMilliseconds;
                Server.Get.Logger.Warn($"Ping to {uid} is {delay}ms");
            }
        }

        public override void StartClient(SynapseNetworkClient client)
        {
            CheckPings();
        }

        public override void ReceiveInstanceMessage(InstanceMessage message)
        {
            Server.Get.Logger.Info("Received InstanceMessage: " + message.Humanize());

            if (message.Subject == "Echo") Server.Get.Logger.Info($"{message.Value()} from {message.Sender}");
            if (message.Subject == "Ping") RespondMessage(message, DateTimeOffset.Now.ToString());
        }

        public override void Heartbeat()
        {
            if (!_checked)
            {
                _checked = true;
                BroadcastMessage("Echo", "Hello!").GetAwaiter();
                //CheckPings();
            }
        }
    }
}