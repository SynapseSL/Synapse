namespace Synapse.Api.Events.SynapseEventArguments
{
    public class TriggerTeslaEventArgs : EventHandler.ISynapseEventArgs
    {
        public Tesla Tesla { get; internal set; }

        public Player Player { get; internal set; }

        public bool HurtRange { get; internal set; }

        public bool Trigger { get; set; }
    }
}
