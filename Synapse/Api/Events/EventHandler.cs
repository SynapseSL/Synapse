namespace Synapse.Api.Events
{
    public class EventHandler
    {
        internal EventHandler() { }

        public delegate void OnSynapseEvent<TEvent>(TEvent ev) where TEvent : ISynapseEventArgs;

        public ServerEvents Server { get; } = new ServerEvents();
        
        public PlayerEvents Player { get; } = new PlayerEvents();

        public RoundEvents Round { get; } = new RoundEvents();
        
        public interface ISynapseEventArgs
        {
        }
    }
}
