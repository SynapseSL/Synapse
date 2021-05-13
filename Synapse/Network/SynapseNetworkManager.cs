using System;
using System.Collections.Generic;
using System.Threading;
using EmbedIO;
using Synapse.Network.Models;

namespace Synapse.Network
{
    public class SynapseNetworkManager
    {
        public SynapseNetworkManager()
        {
            NetworkNodes.Add(new SynapseNetworkNode());
        }

        public List<NetworkNodeBase> NetworkNodes { get; } = new List<NetworkNodeBase>();
        public SynapseNetworkClient Client { get; private set; }
        public ReconnectLoop ReconnectLoop { get; private set; }
        public SynapseNetworkServer Server { get; private set; }
        
        public DateTime Startup { get; private set; }
        public Dictionary<string, ClientSession> ClientSessionTokens { get; set; } =
            new Dictionary<string, ClientSession>();

        public void Start()
        {
            Startup = DateTime.Now;
            ClientSessionTokens.Clear();
            var synapseConfig = Synapse.Server.Get.Configs.synapseConfiguration;
            if (!synapseConfig.NetworkEnable) return;
            Client = new SynapseNetworkClient
            {
                ClientName = synapseConfig.NetworkName,
                Secret = synapseConfig.NetworkSecret,
                Url = synapseConfig.NetworkUrl
            };
            Client.Init();
            ReconnectLoop = new ReconnectLoop(synapseConfig);
            Synapse.Server.Get.Logger.Info("Synapse-Network Starting ClientLoop");
            ReconnectLoop.Start(true);
        }

        public void Shutdown()
        {
            Synapse.Server.Get.Logger.Info("Synapse-Network Shutdown");
            ReconnectLoop.Stop();
            Client.Disconnect();
            Server.Stop();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void BoostrapServer(string publicIp)
        {
            var synapseConfig = Synapse.Server.Get.Configs.synapseConfiguration;
            if (!synapseConfig.NetworkEnable || !synapseConfig.MasterAuthority) return;
            Synapse.Server.Get.Logger.Info("Starting Synapse-Network Server");
            Server = SynapseNetworkServer.GetServer;
            Server.PublicEndpoint = publicIp;
            Server.Secret = synapseConfig.NetworkSecret;
            Server.Port = synapseConfig.NetworkPort;
            Server.Url = synapseConfig.NetworkUrl;
            Server.Init();
            if (!SynapseNetworkServer.CheckServerPortAvailable(synapseConfig.NetworkPort))
            {
                Synapse.Server.Get.Logger.Error(
                    $"Port-Availability check for {synapseConfig.NetworkPort} failed: Port already in use");
                return;
            }

            Server.Start();
            while (Server.Status == WebServerState.Loading) Thread.Sleep(10);
            Synapse.Server.Get.Logger.Info($"Synapse-Network Server running on port {synapseConfig.NetworkPort}");
        }
    }
}