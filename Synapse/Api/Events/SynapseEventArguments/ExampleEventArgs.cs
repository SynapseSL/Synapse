namespace Synapse.Api.Events.SynapseEventArguments
{
    public class ExampleEventArgs : EventHandler.ISynapseEventArgs
    {
        public bool Allow { get; set; } = true;
    }
}
