using System;
using System.Collections.Generic;
using System.Threading;
using EmbedIO;
using Synapse.Network.nodes;

namespace Synapse.Network
{
    public class SynapseNetworkManager
    {
        public readonly List<INetworkNode> NetworkNodes = new List<INetworkNode>();
        private Thread _reconnectLoop;

        public SynapseNetworkClient Client;
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
            Synapse.Server.Get.Logger.Info("Synapse-Network Starting ClientLoop");
            StartClientReconnectLoop(synapseConfig.NetworkPollRate);
        }

        public void Shutdown()
        {
            Synapse.Server.Get.Logger.Info("Synapse-Network Shutdown");
            _reconnectLoop.Abort();
            Client.Disconnect();
            Server.Stop();
        }

        private void StartClientReconnectLoop(long pollRate)
        {
            _reconnectLoop = new Thread(async () =>
            {
                while (true)
                {
                    // Wait for 5.0 seconds
                    var poll = await Client.PollServer();
                    if (poll == null)
                    {
                        Synapse.Server.Get.Logger.Warn("Master-Ping failed");

                        if (Synapse.Server.Get.Configs.synapseConfiguration.MasterAuthority)
                        {
                            BoostrapServer();
                            Thread.Sleep(250);
                            continue;
                        }

                        if (Client.IsStarted)
                        {
                            Client.Disconnect();
                            Synapse.Server.Get.Logger.Warn("Synapse-Network client can't connect to Synapse-Network");
                            //Thread.Sleep(500 * Client.MigrationPriority);
                        }
                    }
                    else if (!Client.IsStarted)
                    {
                        Client.Connect();
                    }
                    else
                    {
                        foreach (var node in NetworkNodes) node.Hearthbeat();
                    }

                    Thread.Sleep(TimeSpan.FromMilliseconds(pollRate));
                }

                // ReSharper disable once FunctionNeverReturns
            });
            _reconnectLoop.Name = "Synapse-Network ReconnectLoop";
            _reconnectLoop.IsBackground = true;
            _reconnectLoop.Start();
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