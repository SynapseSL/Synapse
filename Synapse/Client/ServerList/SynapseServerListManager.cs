using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;
using Swan;
using Swan.Formatters;
using Synapse.Network;
using Logger = Synapse.Api.Logger;

namespace Synapse.Client.ServerList
{
    public class SynapseServerListManager
    {
        internal SynapseServerListManager() { }

        public bool IsVerified { get; private set; } = false;

        private string Token { get; set; }

        public Thread ServerListThread { get; private set; }

        internal void RunServerListThread()
        {
            IsVerified = File.Exists(Server.Get.Files.ServerTokenFile);

            if (IsVerified)
            {
                Token = File.ReadAllText(Server.Get.Files.ServerTokenFile);

                ServerListThread = new Thread(new ThreadStart(SynapseServerList))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal,
                    Name = "SCP:SL Server list thread"
                };

                ServerListThread.Start();
            }
        }

        private async void SynapseServerList()
        {
            Logger.Get.Send("Synapse-Verification: Your Server will be displayed on the Synapse Server List!", ConsoleColor.Green);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SynapseServerClient");
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Api-Key", Token);

            var url = ClientManager.ServerList + "/serverlist";

            for (; ; )
            {
                try
                {
                    var data = new StringContent(Json.Serialize(new SynapseServerListMark
                    {
                        OnlinePlayers = ServerConsole.PlayersAmount,
                        MaxPlayers = Server.Get.Slots,
                        Info = Base64.ToBase64String(ServerConsole.singleton.RefreshServerName().ToBytes())
                    }));

                    await client.PostAsync(url, data);
                    Logger.Get.Warn("Post done");
                }
                catch (Exception e)
                {
                    Logger.Get.Error("Synapse-ServerList: mark server to serverlist failed:\n" + e);
                }

                await Task.Delay(30000);
            }
        }
    }
}
