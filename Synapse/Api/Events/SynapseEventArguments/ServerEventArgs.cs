using LiteNetLib;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class PreAuthenticationEventArgs : EventHandler.ISynapseEventArgs
    {
        public string UserId { get; internal set; }
        public bool Allow { get; set; }
        public ConnectionRequest Request { get; internal set; }
        
        /// <summary>
        /// This field is only required if Allow = false;
        /// </summary>
        public string Reason { get; set; }
    }
}