using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Mirror;
using Swan;
using Synapse.Network.nodes;
using UnityEngine;

namespace Synapse.Network
{
    public class SynapseNetworkManager
    {

        public SynapseNetworkClient Client;
        public SynapseNetworkServer Server;
        private Thread _reconnectLoop;

        public readonly List<INetworkNode> NetworkNodes = new List<INetworkNode>();

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
            StartClientReconnectLoop();
        }

        public void Shutdown()
        {
            Synapse.Server.Get.Logger.Info("Synapse-Network Shutdown");
            _reconnectLoop.Abort();
            Client.Disconnect();
            Server.Stop();
        }

        private void StartClientReconnectLoop()
        {
            _reconnectLoop = new Thread(async () =>
            {
                while (true)
                {
                    // Wait for 5.0 seconds
                    var availability = await Client.CheckAvailability();
                    if (!availability)
                    {

                        if (Synapse.Server.Get.Configs.synapseConfiguration.MasterAuthority)
                        {
                            BoostrapServer();
                            Thread.Sleep(250);
                            continue;
                        }
                        
                        if (Client.IsStarted)
                        {
                            Client.Disconnect();
                            Synapse.Server.Get.Logger.Warn("Synapse-Network client can't connect to Synapse-Network. Retrying in 5 seconds");
                            //Thread.Sleep(500 * Client.MigrationPriority);
                        }
                    }
                    else if (!Client.IsStarted)
                    {
                        Client.Connect();
                    }

                    Thread.Sleep(5000);
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
            Synapse.Server.Get.Logger.Info($"Starting Synapse-Network Server");
            Server = SynapseNetworkServer.Instance;
            Server.Secret = synapseConfig.NetworkSecret;
            Server.Port = synapseConfig.NetworkPort;
            Server.Url = synapseConfig.NetworkUrl;
            Server.Stop();
            if (!SynapseNetworkServer.CheckServerPortAvailable(synapseConfig.NetworkPort))
            {
                Synapse.Server.Get.Logger.Error($"Port-Availability check for {synapseConfig.NetworkPort} failed: Port already in use");
                return;
            }
            Server.Start();
            Synapse.Server.Get.Logger.Info($"Synapse-Network Server running on port {synapseConfig.NetworkPort}");
        }
    }
}