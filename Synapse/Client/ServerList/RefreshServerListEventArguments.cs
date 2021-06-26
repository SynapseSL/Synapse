namespace Synapse.Client.ServerList
{
    public class RefreshServerListEventArguments : Synapse.Api.Events.EventHandler.ISynapseEventArgs
    {
        public SynapseServerListMark Data { get; set; }

        public string Url { get; internal set; }
    }
}
