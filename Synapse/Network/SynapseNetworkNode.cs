using System;
using System.Linq;
using EmbedIO;
using Synapse.Network.Models;
using Synapse.Network.Routes;

namespace Synapse.Network
{
    public class SynapseNetworkNode : NetworkNodeBase
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
            //Debug
            CheckPings();
        }

        public override void ReceiveInstanceMessage(InstanceMessage message)
        {
            switch (message.Subject)
            {
                case "Echo":
                    Server.Get.Logger.Send($"'{message.Value()}' from {message.Sender}", ConsoleColor.White);
                    break;
                case "Ping":
                    RespondMessage(message, DateTimeOffset.Now.ToString());
                    break;
                case "GetPlayer":
                    var netPlayerId = message.Value<string>();
                    var results = Server.Get.GetPlayers(x => x.UserId == netPlayerId);
                    if (!results.IsEmpty()) RespondMessage(message, NetworkPlayer.FromLocalPlayer(results.First()));
                    break;
                case "GetPlayers":
                    RespondMessage(message, Server.Get.Players.Select(NetworkPlayer.FromLocalPlayer).ToList());
                    break;
            }

            //Debug
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