using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan;
using Swan.Formatters;
using Synapse.Api;
using Synapse.Network.Models;

namespace Synapse.Network.Routes
{
    public class SynapseNetworkSyncController : WebApiController
    {
        [Route(HttpVerbs.Get, "/")]
        public IStatus Get([QueryField("key")] string key)
        {
            if (key == null)
                try
                {
                    var clientData = this.GetClientData();
                    if (clientData == null) return StatusedResponse.Unauthorized;
                    return new StatusListWrapper<NetworkSyncEntry>(SynapseNetworkServer.GetServer.NetworkSyncEntries);
                }
                catch (InvalidOperationException e)
                {
                    return StatusedResponse.Unauthorized;
                }

            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) return StatusedResponse.Unauthorized;
                return SynapseNetworkServer.GetServer.NetworkSyncEntries.First(x => x.Key == key);
            }
            catch (InvalidOperationException e)
            {
                return StatusedResponse.NotFound;
            }
        }

        [Route(HttpVerbs.Post, "/")]
        public async Task<StatusedResponse> Set([QueryField("key")] string key)
        {
            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) return StatusedResponse.Unauthorized;
                var syncVar = await HttpContext.GetRequestDataAsync<NetworkSyncEntry>();
                syncVar.Key = key;
                var syncEntries = SynapseNetworkServer.GetServer.NetworkSyncEntries;
                if (syncEntries.Contains(syncVar))
                {
                    syncEntries.Remove(syncVar);
                    if (syncVar.Data.Trim() != "") syncEntries.Add(syncVar);
                }

                return StatusedResponse.Success;
            }
            catch (Exception e)
            {
                return new ErrorStatus("");
            }
        }

        [Route(HttpVerbs.Get, "/client/{id}")]
        public IStatus GetClient(string id, [QueryField("key")] string key)
        {
            if (key == null)
                try
                {
                    var clientData = this.GetClientData();
                    if (clientData == null) return new NotFoundStatus();
                    var target = SynapseNetworkServer.GetServer.DataById(id);
                    if (target == null) return new NotFoundStatus();
                    return new StatusListWrapper<KeyValueObjectWrapper>(target.SyncEntries);
                }
                catch (InvalidOperationException e)
                {
                    return StatusedResponse.NotFound;
                }

            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) return StatusedResponse.Unauthorized;
                return SynapseNetworkServer.GetServer.NetworkSyncEntries.First(x => x.Key == key);
            }
            catch (InvalidOperationException e)
            {
                return StatusedResponse.NotFound;
            }
        }

        [Route(HttpVerbs.Post, "/client")]
        public async Task<StatusedResponse> Publish()
        {
            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) return StatusedResponse.Unauthorized;
                var body = await HttpContext.GetRequestBodyAsStringAsync();
                var syncVar = Json.Deserialize<List<KeyValueObjectWrapper>>(body);
                clientData.SyncEntries = syncVar.ToHashSet();
                var sha256 = SHA256.Create();
                clientData.SyncEntriesHash =
                    sha256.ComputeHash(Json.Serialize(clientData.SyncEntries).ToBytes()).ToLowerHex();
                sha256.Dispose();
#if DEBUG
                Logger.Get.Info($"New ClientVars from {clientData.ClientUid} with hash {clientData.SyncEntriesHash}");
                var server = SynapseNetworkServer.GetServer;
                foreach (var data in server.SyncedClientList.Keys.Select(x => server.DataById(x)))
                    Logger.Get.Info($"{data.ClientUid}: {data.SyncEntries.Humanize()}");
#endif
                return StatusedResponse.Success;
            }
            catch (Exception e)
            {
                return new ErrorStatus("");
            }
        }
    }
}