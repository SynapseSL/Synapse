namespace Synapse.Api.Events.SynapseEventArguments
{
    public class ExampleEventArgs : EventHandler.SynapseEventArgs
    {
        public bool Allow { get; set; } = true;

        public void LogInConsole() => SynapseController.Server.Logger.Info($"ExampleEvent Log Request: Allow:{Allow}");
    }
}
