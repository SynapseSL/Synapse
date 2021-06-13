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

        public static bool IsSynapseClientEnabled { get; private set; } = false;

        public SpawnController SpawnController { get; set; } = new SpawnController();

        public ClientConnectionData DecodeJWT(string jwt)
        {
            var webClient = new WebClient();
            var pem = webClient.DownloadString("https://central.synapsesl.xyz/session/verificationKey");
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
                            webClient.UploadString("https://servers.synapsesl.xyz/serverlist", Json.Serialize(new SynapseServerListMark
                            {
                                onlinePlayers = new Random().Next(0, 31),
                                maxPlayers = 30,
                                info = Base64.ToBase64String("=====> Nordholz.Games <=====\nSynapse Modded Client Server\nFriendlyFire: Active".ToBytes())
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
            public int onlinePlayers { get; set; }
            public int maxPlayers { get; set; }
            public string info { get; set; }
        }

        public Dictionary<String, ClientConnectionData> Clients { get; set; } =
            new Dictionary<string, ClientConnectionData>();
    }


    public class ClientConnectionData
    {
        //JWT Subject == Name
        public string sub { get; set; }
        //JWT Audience == Connected Server
        public string aud { get; set; }
        //JWT Issuer == Most likely Synapse
        public string iss { get; set; }
        public string uuid { get; set; }
        public string session { get; set; }
    }
}