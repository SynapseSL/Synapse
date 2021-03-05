using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Synapse.Network
{
    public class SynapseNetworkSyncController : WebApiController
    {
        [Route(HttpVerbs.Get, "/")]
        public StatusMessage Get([QueryField("key")] string key)
        {
            if (key == null)
            {
                try
                {
                    var clientData = this.GetClientData();
                    if (clientData == null) return new NotFoundStatus();
                    return new StatusListWrapper<NetworkSyncEntry>(SynapseNetworkServer.Instance.NetworkSyncEntries);
                }
                catch (InvalidOperationException e)
                {
                    return StatusMessage.NotFound;
                }
            }
            
            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) throw new HttpException(HttpStatusCode.Unauthorized);
                return SynapseNetworkServer.Instance.NetworkSyncEntries.First(x => x.Key == key);
            }
            catch (InvalidOperationException e)
            {
                return null;
            }
        }
        
        [Route(HttpVerbs.Post, "/")]
        public async Task<StatusMessage> Set([QueryField("key")] string key)
        {
            try
            {
                var clientData = this.GetClientData();
                if (clientData == null) return StatusMessage.Unauthorized;
                var syncVar = await HttpContext.GetRequestDataAsync<NetworkSyncEntry>();
                syncVar.Key = key;
                var syncEntries = SynapseNetworkServer.Instance.NetworkSyncEntries;
                if (syncEntries.Contains(syncVar))
                {
                    syncEntries.Remove(syncVar);
                    if (syncVar.Data.Trim() != "") syncEntries.Add(syncVar);
                } 
                return StatusMessage.Success;
            }
            catch (Exception e)
            {
                return new ErrorStatus("");
            }
        }
    }
}