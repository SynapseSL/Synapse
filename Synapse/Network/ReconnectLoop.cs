using System;
using System.Threading.Tasks;
using EmbedIO;
using Synapse.Config;
using Synapse.Reactive;

namespace Synapse.Network
{
    public class ReconnectLoop : JavaLikeThread
    {
        private readonly SynapseConfiguration _configuration;

        public ReconnectLoop(SynapseConfiguration configuration)
        {
            _configuration = configuration;
        }

        public NotificationSubject PollCycleSubject { get; } = new NotificationSubject();

        public override async void Run()
        {
            while (true)
            {
                var networkManager = Server.Get.NetworkManager;
                var networkClient = networkManager.Client;
                var poll = await networkClient.PollServer();
                try
                {
                    PollCycleSubject.Notify();
                }
                catch (Exception e)
                {
                    Server.Get.Logger.Error(e);
                }

                if (poll == null || !poll.Authenticated)
                {
                    Server.Get.Logger.Warn("Master-Ping failed");

                    if (Server.Get.Configs.synapseConfiguration.MasterAuthority
                        && SynapseNetworkServer.Instance.Status != WebServerState.Loading
                        && SynapseNetworkServer.Instance.Status != WebServerState.Listening
                        && SynapseNetworkServer.CheckServerPortAvailable(_configuration.NetworkPort))
                    {
                        Server.Get.NetworkManager.BoostrapServer();
                        await Task.Delay(TimeSpan.FromMilliseconds(250));
                        continue;
                    }

                    switch (networkClient.IsStarted)
                    {
                        case true:
                            networkClient.Disconnect();
                            Server.Get.Logger.Warn("Synapse-Network client can't connect to Synapse-Network");
                            //Thread.Sleep(500 * Client.MigrationPriority);
                            break;
                        case false:
                            await networkClient.Connect();
                            break;
                    }
                }
                else
                {
                    foreach (var node in networkManager.NetworkNodes) node.Heartbeat();
                }

                await Task.Delay(TimeSpan.FromMilliseconds(_configuration.NetworkPollRate));
            }
        }

        public Task AwaitOneClientPollCycle()
        {
            return new OneShotConsumer<object>(PollCycleSubject).Consume();
        }
    }
}