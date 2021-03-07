using System.Collections.Generic;
using System.Threading.Tasks;
using EmbedIO;

namespace Synapse.Network
{
    public abstract class NetworkNodeBase
    {
        public abstract void RegisterWebserverWith(WebServer server);

        /// <summary>
        ///     Will be called after both client and possible server are started
        /// </summary>
        /// <param name="authority"></param>
        public abstract void Reconfigure(InstanceAuthority authority);

        /// <summary>
        ///     Will be called after the client connects to an servers ignoring the own authority
        /// </summary>
        /// <param name="client"></param>
        public abstract void StartClient(SynapseNetworkClient client);

        public abstract void ReceiveInstanceMessage(InstanceMessage message);

        /// <summary>
        ///     Will be called after each poll-cycle starting after the first poll.
        ///     If the client reconnects or changes authority, the heartbeat will be skipped
        /// </summary>
        public virtual void Heartbeat()
        {
        }

        #region QOL Methods (And Shortcuts)

        public async Task<InstanceMessageTransmission> RespondMessage(InstanceMessage message, object value,
            string subj = null)
        {
            return await Server.Get.NetworkManager.Client.SendMessage(message.CreateResponse(value, subj));
        }

        public async Task<InstanceMessage> SendMessageAndAwaitResponse(InstanceMessage message, string subject)
        {
            return await Server.Get.NetworkManager.Client.SendMessageAndAwaitResponse(message, subject);
        }

        public async Task<InstanceMessage> SendMessageAndAwaitResponse(InstanceMessage message)
        {
            return await Server.Get.NetworkManager.Client.SendMessageAndAwaitResponse(message, message.Subject + "Res");
        }

        public async Task<List<string>> GetClients()
        {
            return await Server.Get.NetworkManager.Client.Get<List<string>>("/synapse/clients");
        }

        public async Task<InstanceMessageTransmission> SendMessage(InstanceMessage message)
        {
            return await Server.Get.NetworkManager.Client.SendMessage(message);
        }

        public async Task<InstanceMessageTransmission> SendMessage(string subj, object value, string recv)
        {
            return await Server.Get.NetworkManager.Client.SendMessage(InstanceMessage.CreateSend(subj, value, recv));
        }

        public async Task<InstanceMessageTransmission> BroadcastMessage(string subj, object value)
        {
            return await Server.Get.NetworkManager.Client.SendMessage(InstanceMessage.CreateBroadcast(subj, value));
        }

        #endregion
    }
}