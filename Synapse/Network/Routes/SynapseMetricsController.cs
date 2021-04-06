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
                    Logger.Get.Info("[1]");
                    var clientData = SynapseNetworkServer.GetServer.DataById(key);
                    Logger.Get.Warn(clientData.Humanize());
                    Logger.Get.Warn(key);
                    Logger.Get.Info("[2]");
                    var entry = clientData.SyncEntries.Get("startupTimestamp");
                    Logger.Get.Info("[3]");
                    var up = clientData.SyncEntries.Get<NetHealthData>("startupTimestamp");
                    Logger.Get.Info(entry.Humanize());
                    nodes.Add(up);
                }
                catch (Exception e)
                {
                    Logger.Get.Error(e.ToString());
                }

                // var response = await SynapseNetworkClient.GetClient.SendMessageAndAwaitResponse(InstanceMessage.CreateSend("Uptime", "", key));
                // var up = (int)response.Parse();

            }

            return new StatusListWrapper<NetHealthData>(nodes);
        }
    }
}