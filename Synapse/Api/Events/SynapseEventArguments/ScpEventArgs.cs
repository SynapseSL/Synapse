namespace Synapse.Api.Events.SynapseEventArguments
{
    public class Scp096AddTargetEventArgument : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player Scp096 { get; internal set; }

        public PlayableScps.Scp096PlayerState RageState { get; internal set; }

        public bool Allow { get; set; }
    }
}