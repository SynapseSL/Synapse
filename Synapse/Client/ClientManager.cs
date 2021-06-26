using System.Collections.Generic;
using Synapse.Client.ServerList;

namespace Synapse.Client
{
    public class ClientManager
    {
        internal ClientManager() { }

        /// <summary>
        /// The URL of the Synapse CentralServer
        /// </summary>
        public const string CentralServer = "https://central.synapsesl.xyz";

        /// <summary>
        /// The URL of the Synapse ServerList
        /// </summary>
        public const string ServerList = "https://servers.synapsesl.xyz";

        /// <summary>
        /// A Boolean that presents whether the Synapse Client feature is activated on this server or not
        /// </summary>
        public bool IsSynapseClientEnabled { get; private set; } = false;

        /// <summary>
        /// The SpawnController for loading prefabs
        /// </summary>
        public SpawnController SpawnController { get; } = new SpawnController();

        /// <summary>
        /// The ServerListManager for sending infos to the Synapse Server List
        /// </summary>
        public SynapseServerListManager ServerListManager { get; } = new SynapseServerListManager();

        /// <summary>
        /// The ConnectionData for all Players on the server
        /// </summary>
        /// <remarks>
        /// It only contains data about players connected with the Synapse Client
        /// </remarks>
        public Dictionary<string, ClientConnectionData> Clients { get; set; } =
            new Dictionary<string, ClientConnectionData>();

        internal void Initialise()
        {
            IsSynapseClientEnabled = Server.Get.Configs.synapseConfiguration.SynapseServerList;

            if (!IsSynapseClientEnabled) return;

            new EventHandlers();

            ServerListManager.RunServerListThread();
        }
    }
}