using System.Collections.Generic;
using System.Threading;
using EmbedIO;

namespace Synapse.Network
{
    public class SynapseNetworkManager
    {
        public readonly List<NetworkNodeBase> NetworkNodes = new List<NetworkNodeBase>();

        public SynapseNetworkClient Client;
        public ReconnectLoop ReconnectLoop;
        public SynapseNetworkServer Server;

        public SynapseNetworkManager()
        {
            NetworkNodes.Add(new SynapseNetworkNode());
        }

        public void Start()
        {
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
        public void BoostrapServer()
        {
            var synapseConfig = Synapse.Server.Get.Configs.synapseConfiguration;
            if (!synapseConfig.NetworkEnable || !synapseConfig.MasterAuthority) return;
            Synapse.Server.Get.Logger.Info("Starting Synapse-Network Server");
            Server = SynapseNetworkServer.Instance;
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