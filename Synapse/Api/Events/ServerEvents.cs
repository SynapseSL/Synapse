namespace Synapse.Api.Events
{
    public class ServerEvents
    {
        public event EventHandler.OnSynapseEvent<SynapseEventArguments.ExampleEventArgs> ExampleEvent;
    }
}
