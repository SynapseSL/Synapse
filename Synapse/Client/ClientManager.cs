﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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

namespace Synapse.Client
{

    public class ClientManager
    {
        internal ClientManager() { }

        public const string CentralServer = "https://central.synapsesl.xyz";

        public const string ServerList = "https://servers.synapsesl.xyz";

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

            new SynapseServerListThread().Run();

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

            ClientPipeline.DataReceivedEvent += delegate (Player player, PipelinePacket ev)
            {
                switch (ev.PacketId)
                {
                    case 1: break;
                    case 10: break; //Ignore as Server
                    case 11: break; //Ignore as Server
                    case 12: break; //Ignore as Server
                }
            };
        }

        public class SynapseServerListThread : JavaLikeThread
        {
            private readonly WebClient webClient = new WebClient();

            public override async void Run()
            {
                for (; ; )
                {
                    try
                    {
                        if (File.Exists(Server.Get.Files.ServerTokenFile))
                        {
                            var token = File.ReadAllText(Server.Get.Files.ServerTokenFile);
                            webClient.Headers.Clear();
                            webClient.Headers.Add("User-Agent", "SynapseServerClient");
                            webClient.Headers.Add("Content-Type", "application/json");
                            webClient.Headers.Add("Api-Key", token);
                            webClient.UploadString(ServerList + "/serverlist", Json.Serialize(new SynapseServerListMark
                            {
                                OnlinePlayers = new Random().Next(0, 31),
                                MaxPlayers = 30,
                                Info = Base64.ToBase64String("=====> Nordholz.Games <=====\nSynapse Modded Client Server\nFriendlyFire: Active".ToBytes())
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Get.Error("Error when trying to mark server to serverlist: " + e.ToString());
                    }
                    await Task.Delay(1000 * 10);
                }
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