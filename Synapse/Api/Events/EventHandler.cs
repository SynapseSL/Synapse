using Synapse.Config;

namespace Synapse.Api.Events
{
    public class EventHandler
    {
        internal EventHandler()
        {
            Player.PlayerJoinEvent += PlayerJoin;
        }

        public delegate void OnSynapseEvent<TEvent>(TEvent ev) where TEvent : ISynapseEventArgs;

        public ServerEvents Server { get; } = new ServerEvents();
        
        public PlayerEvents Player { get; } = new PlayerEvents();

        public RoundEvents Round { get; } = new RoundEvents();
        
        public interface ISynapseEventArgs
        {
        }

        #region HookedMethods
        private SynapseConfiguration conf => SynapseController.Server.Configs.SynapseConfiguration;

        private void PlayerJoin(SynapseEventArguments.PlayerJoinEventArgs ev)
        {
            ev.Player.Broadcast(conf.JoinMessagesDuration, conf.JoinBroadcast);
            ev.Player.Broadcast(conf.JoinMessagesDuration, conf.JoinTextHint);
        }

        //TODO: Invoke Sync Event to check other Roles for escaping
        #endregion
    }
}
