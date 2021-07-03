using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Swan.Formatters;
using Logger = Synapse.Api.Logger;

namespace Synapse.Client.ServerList
{
    public class SynapseServerListManager
    {
        internal SynapseServerListManager() { }

        /// <summary>
        /// The Informations that should be send to the server list server
        /// </summary>
        public ServerListInfo Info { get; } = new ServerListInfo();

        /// <summary>
        /// If a token Exist
        /// </summary>
        /// <remarks>
        /// Is also true when the token is invalid
        /// </remarks>
        public bool IsVerified { get; private set; } = false;

        private string Token { get; set; }

        /// <summary>
        /// The Thread for sending the Informations to the server list
        /// </summary>
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
            await Task.Delay(500);

            Logger.Get.Send("Synapse-Verification: Your Server will be displayed on the Synapse Server List!", ConsoleColor.Green);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Api-Key", Token);

            var url = ClientManager.ServerList + "/serverlist";

            for (; ; )
            {
                try
                {
                    var mark = Info.GetMark();
                    var data = new StringContent(Json.Serialize(mark), Encoding.UTF8, "application/json");

                    await client.PostAsync(url, data);

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
