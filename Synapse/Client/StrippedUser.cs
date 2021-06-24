using System.Collections.Generic;
using Synapse.Permission;
using Newtonsoft.Json;
using System.Net;

namespace Synapse.Client
{
    public class StrippedUser
    {
        private StrippedUser() { }

        public static StrippedUser Resolve(string uid, string session)
        {
            var webclient = new WebClient();
            webclient.Headers["Authorization"] = $"Bearer {session}";
            var url = ClientManager.CentralServer + $"/public/{uid}";
            var response = webclient.DownloadString(url);
            var user = JsonConvert.DeserializeObject<StrippedUser>(response);
            return user;
        }

        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("groups")]
        public List<GlobalSynapseGroup> Groups { get; set; }
    }
}
