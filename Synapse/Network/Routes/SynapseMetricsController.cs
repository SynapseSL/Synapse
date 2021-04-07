using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Extensions.Logging;
using Swan;
using Swan.Formatters;
using Synapse.Api;
using Synapse.Network.Models;

namespace Synapse.Network.Routes
{
    public class SynapseMetricsController : WebApiController
    {
        [Route(HttpVerbs.Get, "/alive")]
        public async Task<IStatus> Health()
        {
            var nodes = new List<NetHealthData>();
            foreach (var key in SynapseNetworkServer.GetServer.SyncedClientList.Keys)
            {
                try
                {
                    var clientData = SynapseNetworkServer.GetServer.DataById(key);;
                    if (clientData != null)
                    {
                        var healthData = clientData.SyncEntries.Get<NetHealthData>("startupTimestamp");
                        nodes.Add(healthData);
                    }
                }
                catch (Exception e)
                {
                    Logger.Get.Error(e.ToString());
                }
            }

            return new StatusListWrapper<NetHealthData>(nodes);
        }
    }
}