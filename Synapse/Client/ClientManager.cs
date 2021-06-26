using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using JWT.Algorithms;
using JWT.Builder;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Swan;
using Swan.Formatters;
using Synapse.Api;
using Synapse.Client.Packets;
using Synapse.Network;
using Logger = Synapse.Api.Logger;
using Random = System.Random;
using System.Text;

namespace Synapse.Client
{

    public class ClientManager
    {
        internal ClientManager() { }

        public const string CentralServer = "https://central.synapsesl.xyz";

        public const string ServerList = "https://servers.synapsesl.xyz";

        public bool IsVerified { get; private set; } = false;

        private string Token { get; set; }

        public static bool IsSynapseClientEnabled { get; private set; } = false;

        public SpawnController SpawnController { get; set; } = new SpawnController();

        public ClientConnectionData DecodeJWT(string jwt)
        {
            var webClient = new WebClient();
            var pem = webClient.DownloadString(CentralServer + "/session/verificationKey");
            var pr = new PemReader(new StringReader(pem));
            var publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);
            var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportParameters(rsaParams);
            var payload = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(rsa))
                .MustVerifySignature()
                .Decode<ClientConnectionData>(jwt);
            return payload;
        }

        internal void Initialise()
        {
            IsSynapseClientEnabled = Server.Get.Configs.synapseConfiguration.SynapseServerList;
            if (!IsSynapseClientEnabled) return;

            IsVerified = File.Exists(Server.Get.Files.ServerTokenFile);
            if (IsVerified)
            {
                Token = File.ReadAllText(Server.Get.Files.ServerTokenFile);
                var thread = new Thread(new ThreadStart(SynapseServerList))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal,
                    Name = "SCP:SL Server list thread"
                };
                thread.Start();
            }


            Server.Get.Events.Round.RoundStartEvent += delegate
            {
                ClientPipeline.InvokeBroadcast(PipelinePacket.From(RoundStartPacket.ID, new byte[0]));
            };

            ClientPipeline.ClientConnectionCompleteEvent += delegate (Player player, ClientConnectionComplete ev)
            {
                if (Round.Get.RoundIsActive)
                {
                    SpawnController.SpawnLate(ev.Player);
                }
            };

            SynapseController.Server.Events.Round.RoundEndEvent += delegate
            {
                SpawnController.SpawnedObjects.Clear();
            };

            Logger.Get.Info("Loading Complete");
        }

        private async void SynapseServerList()
        {
            Logger.Get.Send("Synapse-Verification: Your Server will be displayed on the Synapse Server List!", ConsoleColor.Green);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SynapseServerClient");
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Api-Key", Token);

            var url = ServerList + "/serverlist";

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
                catch(Exception e)
                {
                    Logger.Get.Error("Synapse-ServerList: mark server to serverlist failed:\n" + e);
                }

                await Task.Delay(10000);
            }
        }

        public class SynapseServerListMark
        {
            [Swan.Formatters.JsonProperty("onlinePlayers")]
            public int OnlinePlayers { get; set; }

            [Swan.Formatters.JsonProperty("maxPlayers")]
            public int MaxPlayers { get; set; }

            [Swan.Formatters.JsonProperty("info")]
            public string Info { get; set; }
        }

        public Dictionary<String, ClientConnectionData> Clients { get; set; } =
            new Dictionary<string, ClientConnectionData>();
    }


    public class ClientConnectionData
    {
        //JWT Subject == Name
        [Newtonsoft.Json.JsonProperty("sub")]
        public string Sub { get; set; }
        //JWT Audience == Connected Server
        [Newtonsoft.Json.JsonProperty("aud")]
        public string Aud { get; set; }
        //JWT Issuer == Most likely Synapse
        [Newtonsoft.Json.JsonProperty("iss")]
        public string Iss { get; set; }
        [Newtonsoft.Json.JsonProperty("uuid")]
        public string Uuid { get; set; }
        [Newtonsoft.Json.JsonProperty("session")]
        public string Session { get; set; }
    }
}