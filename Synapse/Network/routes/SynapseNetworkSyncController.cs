using System;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Synapse.Network
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
                    if (clientData == null) return new NotFoundStatus();
                    return new StatusListWrapper<NetworkSyncEntry>(SynapseNetworkServer.Instance.NetworkSyncEntries);
                }
                catch (InvalidOperationException e)
                {
                    return StatusedResponse.NotFound;
                }

            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) return StatusedResponse.Unauthorized;
                return SynapseNetworkServer.Instance.NetworkSyncEntries.First(x => x.Key == key);
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
                var syncEntries = SynapseNetworkServer.Instance.NetworkSyncEntries;
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
    }
}