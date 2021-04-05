using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Swan;
using Swan.Formatters;
using Synapse.Api;
using Synapse.Config;
using Synapse.Network.Models;

namespace Synapse.Network
{
    public class ClientSyncEntryThread : JavaLikeThread
    {
        private readonly SynapseConfiguration _configuration;

        public ClientSyncEntryThread(SynapseConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override async void Run()
        {
            while (true)
                try
                {
                    var client = SynapseNetworkClient.GetClient;
                    client.SyncEntries.Set("players",
                        Server.Get.Players.Select(NetworkPlayer.FromLocalPlayer).ToList());
                    client.SyncEntries.Set("maxplayers", Server.Get.Slots);

                    var sha256 = SHA256.Create();
                    var entriesClone = client.SyncEntries.ToList();
                    var hash = sha256.ComputeHash(Json.Serialize(entriesClone).ToBytes()).ToLowerHex();
                    sha256.Dispose();
                    if (client.LastSyncEntriesHash != hash)
                    {
                        var response = await client.Post<StatusedResponse, List<KeyValueObjectWrapper>>(
                            "/networksync/client", entriesClone);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(_configuration.NetworkPollRate));
                }
                catch (Exception e)
                {
                    Logger.Get.Error(e);
                }
        }
    }
}